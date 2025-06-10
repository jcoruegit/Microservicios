using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;

namespace MicroRabbit.Transfer.Domain.EventHandlers
{
    /*Manejador de eventos de TransferCreatedEvent*/
    public class TransferEventHandler : IEventHandler<TransferCreatedEvent>
    {
        private readonly ITransferRepository _transferRepository;
        
        public TransferEventHandler(ITransferRepository transferRepository)
        {
            _transferRepository = transferRepository;
        }


        /*Se ejecuta cuando llega un mensaje en la cola y lo consume*/
        public Task Handle(TransferCreatedEvent @event)
        {
            var transaction = new TransferLog
            {
                FromAccount = @event.From,
                ToAccount = @event.To,
                TransferAmount = @event.Amount
            };
            
            _transferRepository.AddTransferLog(transaction);
            
            return Task.CompletedTask;
        }
    }
}
