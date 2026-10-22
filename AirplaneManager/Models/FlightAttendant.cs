using System;
using System.Collections.Generic;

namespace AirplaneManager.Models
{
    public class FlightAttendant : Person
    {
        private int _service;
        private List<string> _languages = new List<string>();

        public int Service
        {
            get => _service;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Стаж работы не может быть отрицательным.");
                _service = value;
            }
        }

        public List<string> Languages
        {
            get => _languages;
            set => _languages = value ?? new List<string>();
        }

        public FlightAttendant() : base() { }

        public FlightAttendant(int id, string firstName, string lastName, int age, string passport, int service, List<string>? languages = null)
            : base(id, firstName, lastName, age, passport)
        {
            Service = service;
            Languages = languages ?? new List<string>();
        }

        // Для новых стюардесс
        public FlightAttendant(string firstName, string lastName, int age, string passport, int service, List<string>? languages = null)
            : base(firstName, lastName, age, passport)
        {
            Service = service;
            Languages = languages ?? new List<string>();
        }
    }
}