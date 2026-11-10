using System;

namespace AirplaneManager.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при попытке посадить на рейс больше пассажиров,
    /// чем вмещает назначенный самолет.
    /// </summary>
    public class FlightCapacityExceededException : Exception
    {
        public int Capacity { get; }
        public int PassengerCount { get; }

        public FlightCapacityExceededException()
            : base("Количество пассажиров превышает вместимость самолета.") { }

        public FlightCapacityExceededException(string message)
            : base(message) { }

        public FlightCapacityExceededException(int capacity, int passengerCount)
            : base($"Вместимость самолета ({capacity} мест) превышена: выбрано {passengerCount} пассажиров.")
        {
            Capacity = capacity;
            PassengerCount = passengerCount;
        }

        public FlightCapacityExceededException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}