using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus
{
    //Interfaz generica para cualquier eventbus (RabbitMQ, Kafka, etc)
    public interface  IEventBus
    {
        Task SendCommand<T>(T command) where T : Command; //Para enviar mensajes de un componente a otro segun el patron Mediator es mediante objetos de tipo Command

        void Publish<T>(T @event) where T : Event; //Publica mensajes en RabbitMQ

        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;

    }
}
