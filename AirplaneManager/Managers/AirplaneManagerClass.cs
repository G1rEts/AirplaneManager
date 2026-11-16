using System;
using System.Collections.Generic;
using System.Text;
using AirplaneManager.Models;

namespace AirplaneManager.Managers
{
    // Хранилище списков сущностей в оперативной памяти
    internal class AirplaneManagerClass
    {
        public List<Airport> Airports { get; set; } = new List<Airport>();
        public List<Airplane> Airplanes { get; set; } = new List<Airplane>();
        public List<Route> Routes { get; set; } = new List<Route>();
        public List<Pilot> Pilots { get; set; } = new List<Pilot>();
        public List<FlightAttendant> FlightAttendants { get; set; } = new List<FlightAttendant>();
        public List <Passenger> Passengers { get; set; } = new List<Passenger>();
        public List<Flight> Flights { get; set; } = new List<Flight>();

        // Конструктор: инициализация пустых списков
        public AirplaneManagerClass() 
        {
            Airports = new List<Airport>();
            Airplanes = new List<Airplane>();
            Routes = new List<Route>();
            Pilots = new List<Pilot>();
            FlightAttendants = new List<FlightAttendant>();
            Passengers = new List<Passenger>();
            Flights = new List<Flight>();
        }

        // Добавление аэропорта в список
        public void AddAirport(Airport airport)
        {
            if (airport != null)
            {
                Airports.Add(airport);
            }
        }

        // Добавление самолета в список
        public void AddAirplane(Airplane airplane)
        {
            if (airplane != null)
            {
                Airplanes.Add(airplane);
            }
        }

        // Добавление маршрута в список
        public void AddRoute(Route route)
        {
            if (route != null)
            {
                Routes.Add(route);
            }
        }

        // Добавление пилота в список
        public void AddPilot(Pilot pilot)
        {
            if (pilot != null)
            {
                Pilots.Add(pilot);
            }
        }

        // Добавление бортпроводника в список
        public void AddFlightAttendant(FlightAttendant flightAttendant)
        {
            if (flightAttendant != null)
            {
                FlightAttendants.Add(flightAttendant);
            }
        }

        // Добавление пассажира в список
        public void AddPassenger(Passenger passenger)
        {
            if (passenger != null)
            {
                Passengers.Add(passenger);
            }
        }

        // Добавление рейса в список
        public void AddFlight(Flight flight)
        {
            if (flight != null)
            {
                Flights.Add(flight);
            }
        }
    }
}
