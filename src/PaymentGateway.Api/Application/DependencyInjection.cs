﻿namespace PaymentGateway.Api.Application;
public static class DependencyInjection
{
    public static void ConfigureApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
    }
}