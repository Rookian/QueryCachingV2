using System.Threading.Tasks;
using MediatR;

namespace QueryCaching.Infrastructure
{
    public class Db : IDb
    {
        private readonly IMediator _mediator;

        public Db(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<TResponse> Query<TResponse>(IDbQuery<TResponse> dbQuery)
        {
            return _mediator.Send(dbQuery);
        }

        public Task Command(IDbCommand dbCommand)
        {
            return _mediator.Send(dbCommand);
        }
    }

    public interface IDbQueryHandler<in TDbQuery, TResponse> : IRequestHandler<TDbQuery, TResponse> where TDbQuery : IRequest<TResponse> { }
    public interface IDbCommandHandler<in TDbCommand> : IRequestHandler<TDbCommand> where TDbCommand : IRequest { }
    public interface IDbQuery<out TResponse> : IRequest<TResponse> { }
    public interface IDbCommand : IRequest { }
    public interface IDb
    {
        Task<TResponse> Query<TResponse>(IDbQuery<TResponse> dbQuery);
        Task Command(IDbCommand dbCommand);
    }
}