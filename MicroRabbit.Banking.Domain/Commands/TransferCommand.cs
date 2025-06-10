using MicroRabbit.Domain.Core.Commands;

namespace MicroRabbit.Banking.Domain.Commands
{
    /*Comunica la API con RabbitMQ*/
    public abstract class TransferCommand : Command
    {
        public int From { get; protected set; }
        public int To { get; protected set; }
        public decimal Amount { get; protected set; }
    }
}
