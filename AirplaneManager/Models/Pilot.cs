using System;

namespace AirplaneManager.Models
{
    public class Pilot : Person
    {
        private int _flightHours;
        private int _licenseNumber;
        private int _rang;

        public int FlightHours
        {
            get => _flightHours;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Налет часов не может быть отрицательным.");
                _flightHours = value;
            }
        }

        public int LicenseNumber
        {
            get => _licenseNumber;
            set
            {
                // Проверяем, что число находится в диапазоне ровно из 6 знаков
                if (value < 100000 || value > 999999)
                    throw new ArgumentException("Номер лицензии пилота должен состоять ровно из 6 цифр (от 100000 до 999999).");

                _licenseNumber = value;
            }
        }
        public int Rang
        {
            get => _rang;
            set
            {
                if (value <= 0 || value >= 3)
                    throw new ArgumentOutOfRangeException(nameof(value), "Ранг должен быть больше 0 и меньше 3.");
                _rang = value;
            }
        }

        public Pilot() : base() { }

        public Pilot(int id, string firstName, string lastName, int age, string passport, int flightHours, int licenseNumber, int rang)
            : base(id, firstName, lastName, age, passport)
        {
            FlightHours = flightHours;
            LicenseNumber = licenseNumber;
            Rang = rang;
        }

        // Конструктор для новых пилотов
        public Pilot(string firstName, string lastName, int age, string passport, int flightHours, int licenseNumber, int rang)
            : base(firstName, lastName, age, passport)
        {
            FlightHours = flightHours;
            LicenseNumber = licenseNumber;
            Rang = rang;
        }
    }

}
