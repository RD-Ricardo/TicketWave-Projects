﻿namespace ProcessPayment.Bus
{
    public interface IBusService
    {
        Task PublishAsync<T>(string routingKey, T message);
    }
}