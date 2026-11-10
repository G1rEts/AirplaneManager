using System;

namespace AirplaneManager.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при попытке создать маршрут,
    /// где пункт вылета совпадает с пунктом назначения.
    /// </summary>
    public class InvalidRouteException : Exception
    {
        public string? DepartureAirport { get; }
        public string? ArrivalAirport { get; }

        public InvalidRouteException()
            : base("Аэропорт вылета не может совпадать с аэропортом назначения.") { }

        public InvalidRouteException(string message)
            : base(message) { }

        public InvalidRouteException(string departureAirport, string arrivalAirport)
            : base($"Некорректный маршрут: пункт вылета '{departureAirport}' совпадает с пунктом назначения '{arrivalAirport}'.")
        {
            DepartureAirport = departureAirport;
            ArrivalAirport = arrivalAirport;
        }

        public InvalidRouteException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
