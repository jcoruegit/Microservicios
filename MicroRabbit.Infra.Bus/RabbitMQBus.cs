using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MicroRabbit.Infra.Bus
{
    public class RabbitMQBus : IEventBus
    {
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers; //manejador de eventos que suceden en el bus
        private readonly List<Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQBus( IMediator mediator, IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> rabbitMQSettings)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            _rabbitMQSettings = rabbitMQSettings.Value;
        }

        public void Publish<T>(T @event) where T : Event
        {
            //Conexion a RabbitMQ
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.Hostname,
                UserName = _rabbitMQSettings.Username,
                Password = _rabbitMQSettings.Password
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                var eventName = @event.GetType().Name; //Nombre de la clase que envia el mensaje

                channel.QueueDeclare(eventName, false, false, false, null); //se declara una cola con el nombre de la clase que envia el mensaje

                var message = JsonConvert.SerializeObject(@event);

                var body = Encoding.UTF8.GetBytes(message); // se envia el mensaje como bytes porque RabbitmQ recibe los mensajes de ese modo

                channel.BasicPublish("", eventName, null, body); // se envia el mensaje

            }
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name; //se obtiene el nombre de la clase que envia el mensaje
            var handlerType = typeof(TH); // se obtiene el objeto de tipo handler

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException($"El handler exception {handlerType.Name} ya fue registrado anteriormente por '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>(); //se consume el mensaje

        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.Hostname,
                UserName = _rabbitMQSettings.Username,
                Password = _rabbitMQSettings.Password,
                DispatchConsumersAsync = true //Dispatch del consumer en forma asincrona
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel); //se instancia el consumidor

            consumer.Received += Consumer_Received; //evento que se ejecuta cuando llega un mensaje

            channel.BasicConsume(eventName, true, consumer); //ya se procesó el mensaje y puede ser retirado de la cola

        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey; //nombre del evento
            var message = Encoding.UTF8.GetString(e.Body.Span); //se captura el mensaje

            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false); //se procesa el mensaje
            }
            catch(Exception ex) { 
            
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subscriptions = _handlers[eventName]; //quienes estan suscriptos a esta cola

                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(subscription);  //Activator.CreateInstance(subscription) para usar el Activator tiene que haber un constructor en blanco, se cambia para usar constructores que reciban parametros ;
                        if (handler == null) continue;
                        var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                        var @event = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType); // obtenemos el consumidor del mensaje

                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event }); //se ejecuta el metodo que procesa el mensaje

                    }

                }

                 
            
            }

        }
    }
}
