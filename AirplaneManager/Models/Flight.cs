using System;
using System.Collections.Generic;

namespace AirplaneManager.Models
{
    public class Flight
    {
        private int _id;
        private DateTime _flightDate;
        private Route _route = null!;
        private Airplane? _airplane;
        private List<Passenger> _passengers = new List<Passenger>();
        private List<Pilot> _pilots = new List<Pilot>();
        private List<FlightAttendant> _flightAttendants = new List<FlightAttendant>();

        public int Id { get => _id; set => _id = value; }
        public DateTime FlightDate { get => _flightDate; set => _flightDate = value; }
        public Route Route
        {
            get => _route;
            set => _route = value ?? throw new ArgumentNullException(nameof(value), "Маршрут рейса не может быть null.");
        }
        public Airplane? Airplane { get => _airplane; set => _airplane = value; }

        public List<Passenger> Passengers
        {
            get => _passengers;
            set => _passengers = value ?? new List<Passenger>();
        }

        public List<Pilot> Pilots
        {
            get => _pilots;
            set => _pilots = value ?? new List<Pilot>();
        }

        public List<FlightAttendant> FlightAttendants
        {
            get => _flightAttendants;
            set => _flightAttendants = value ?? new List<FlightAttendant>();
        }

        public Flight() { }

        public Flight(int id, DateTime flightDate, Route route, Airplane? airplane = null,
                      List<Passenger>? passengers = null, List<Pilot>? pilots = null, List<FlightAttendant>? flightAttendants = null)
        {
            Id = id;
            FlightDate = flightDate;
            Route = route;
            Airplane = airplane;
            Passengers = passengers ?? new List<Passenger>();
            Pilots = pilots ?? new List<Pilot>();
            FlightAttendants = flightAttendants ?? new List<FlightAttendant>();
        }

        public Flight(DateTime flightDate, Route route, Airplane? airplane = null,
                      List<Passenger>? passengers = null, List<Pilot>? pilots = null, List<FlightAttendant>? flightAttendants = null)
            : this(0, flightDate, route, airplane, passengers, pilots, flightAttendants) { }
    }
}