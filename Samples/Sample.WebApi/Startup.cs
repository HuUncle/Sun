using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.WebApi.Events;
using Sun.EventBus.Abstractions;
using Sun.EventBus.Extensions;
using Sun.EventBus.RabbitMQ.Extensions;

namespace Sample.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TestAEventHandler>();
            services.AddSingleton<TestBEventHandler>();

            services.AddControllers();

            services.Configure<RabbitMQOption>(Configuration.GetSection("RabbitMQOption"));

            //services.AddSingleton<LogEventHandler>();

            services.AddEventBus(x => x.UseRabbitMQ());

            //services.AddSingleton<ILogStore, LogStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //var store = app.ApplicationServices.GetService<ILogStore>();

            //loggerFactory.AddProvider(new SunLoggerProvider(store));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var bus = app.ApplicationServices.GetService<IEventBus>();
            bus.Subscribe<TestAEvent, TestAEventHandler>();
            bus.Subscribe<TestBEvent, TestBEventHandler>();

            //bus.Subscribe<LogEvent, LogEventHandler>();
            bus.StartSubscribe();
        }
    }
}