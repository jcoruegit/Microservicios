using MediatR;

namespace MicroRabbit.Domain.Core.Events
{
    public abstract class Message : IRequest<bool>
    {
        public string MessageType { get; protected set; }

        protected Message()
        {
            MessageType = GetType().Name; //Obtiene el nombre de la clase que esta ejecutando este mensaje (reflection)
        }
    }
}
