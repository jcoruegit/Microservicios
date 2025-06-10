using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus
{
    public interface IEventHandler<in TEvent> : IEventHandler 
        where TEvent : Event
    {
        Task Handle(TEvent @event); //Realiza las operaciones sobre los eventos(mensajes)
    }

    public interface IEventHandler { }
}
