using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Banking.Domain.Events
{
    /*Representa el routing key (declaracion del queue) dentro de RabbitMQ. A quien le envio el dinero*/
    public  class TransferCreatedEvent : Event
    {
        public int From { get; set; }
        public int To { get; set; }
        public decimal Amount { get; set; }

        public TransferCreatedEvent(int from, int to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }
    }
}
