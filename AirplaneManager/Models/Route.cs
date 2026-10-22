using System;

namespace AirplaneManager.Models
{
    public class Route
    {
        private int _id;
        private string _routeNumber = string.Empty;
        private double _flightDurationHours;
        private Airport _departureAirport = null!;
        private Airport _arrivalAirport = null!;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string RouteNumber
        {
            get => _routeNumber;
            set => _routeNumber = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Номер маршрута не может быть пустым.", nameof(value))
                : value;
        }

        public double FlightDurationHours
        {
            get => _flightDurationHours;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Длительность полета должна быть меньше 0.");
                _flightDurationHours = value;
            }
        }

        public Airport DepartureAirport
        {
            get => _departureAirport;
            set => _departureAirport = value ?? throw new ArgumentNullException(nameof(value), "Аэропорт вылета не может быть null.");
        }

        public Airport ArrivalAirport
        {
            get => _arrivalAirport;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Аэропорт прилета не может быть null.");

                if (_departureAirport != null && value.Code == _departureAirport.Code)
                    throw new ArgumentException("Аэропорт прилета не может совпадать с аэропортом вылета.");

                _arrivalAirport = value;
            }
        }

        public Route() { }

        public Route(int id, string routeNumber, double flightDurationHours, Airport departureAirport, Airport arrivalAirport)
        {
            Id = id;
            RouteNumber = routeNumber;
            FlightDurationHours = flightDurationHours;
            DepartureAirport = departureAirport;
            ArrivalAirport = arrivalAirport;
        }

        public Route(string routeNumber, double flightDurationHours, Airport departureAirport, Airport arrivalAirport)
            : this(0, routeNumber, flightDurationHours, departureAirport, arrivalAirport) { }

        public override string ToString() => $"{RouteNumber} ({DepartureAirport?.Code} -> {ArrivalAirport?.Code})";
    }
}