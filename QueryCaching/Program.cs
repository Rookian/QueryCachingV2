using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using QueryCaching.Infrastructure;
using SimpleInjector;

namespace QueryCaching
{
    /*
     * Caching
     *
     * Anforderungen: CustomerById soll direkt bei Appliaktionsstart gecacht werden und später bei Änderung des Modified Dates automatisch aktualisiert werden.
     * Ideen:
     *  - Bei AppStart alle Customer laden und CustomerById für alle geladenen CustomerAufrufen und die Aufrufe cachen
     *  - Woher weiß man welche QueryHandler man aufrufen muss bei Applikationsstart bzw. wie ist die Verbindung?
     *   - Ideen:
     *     - Rückgabe Typ der Query nutzen (Muss IEnumerable<T> oder T sein)
     *     - Explizite Konfiguration von Query zu CacheUpdater/Aufbauer     -> Cache<CustomerCache>().CacheQuery<GetCustomerByIdQuery>().CacheQuery<GetCustomersQuery>
     *
     *
     */
    class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
        public static async Task MainAsync(string[] args)
        {
            var container = new Container();
            var currentAssembly = typeof(Program).Assembly;


            container.Register<IMediator, Mediator>();
            container.Register(typeof(IRequestHandler<,>), currentAssembly);
            container.Register(typeof(IRequestHandler<>), currentAssembly);
            container.RegisterInstance(Console.Out);
            container.RegisterInstance(new SingleInstanceFactory(container.GetInstance));
            container.RegisterInstance(new MultiInstanceFactory(container.GetAllInstances));

            //Pipeline
            container.RegisterCollection(typeof(IPipelineBehavior<,>), Type.EmptyTypes);
            container.RegisterCollection(typeof(IRequestPreProcessor<>), currentAssembly);
            container.RegisterCollection(typeof(IRequestPostProcessor<,>), currentAssembly);

            container.Register(typeof(IDbQueryHandler<,>), currentAssembly);
            container.Register(typeof(IDbCommandHandler<>), currentAssembly);
            container.Register<IDb, Db>();

            var db = container.GetInstance<IDb>();
            var customers = await db.Query(new GetCustomerByIdQuery());
        }
    }

    public class GetCustomerByIdQuery : IDbQuery<Customer>
    {

    }

    public class GetCustomersQueryCacheHandler : IDbQueryHandler<GetCustomerByIdQuery, Customer>
    {
        private readonly IDbQueryHandler<GetCustomerByIdQuery, Customer> _inner;

        public GetCustomersQueryCacheHandler(IDbQueryHandler<GetCustomerByIdQuery, Customer> inner)
        {
            _inner = inner;
        }

        public async Task<Customer> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customers = await _inner.Handle(request, cancellationToken);
            return customers;
        }
    }

    public class GetCustomerByIdQueryHandler : IDbQueryHandler<GetCustomerByIdQuery, Customer>
    {
        public async Task<Customer> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);

            return new Customer { Id = 1, Name = "Microsoft" };
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
