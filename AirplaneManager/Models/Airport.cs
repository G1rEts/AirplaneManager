using System;

namespace AirplaneManager.Models
{
    public class Airport
    {
        private int _id;
        private string _name = string.Empty;
        private string _code = string.Empty;
        private string _city = string.Empty;
        private string _country = string.Empty;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Название аэропорта не может быть пустым.");
                _name = value.Trim();
            }
        }

        public string Code
        {
            get => _code;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Trim().Length != 3)
                    throw new ArgumentException("Код аэропорта должен состоять ровно из 3 символов (например, KJA).");
                _code = value.Trim().ToUpper();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Город не может быть пустым.");
                _city = value.Trim();
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Страна не может быть пустой.");
                _country = value.Trim();
            }
        }

        public Airport() { }

        public Airport(int id, string name, string code, string city, string country)
        {
            Id = id;
            Name = name;
            Code = code;
            City = city;
            Country = country;
        }

        // Новый аэропрт
        public Airport(string name, string code, string city, string country)
            : this(0, name, code, city, country) { }

        public override string ToString() => $"{City} ({Code}) - {Name}";
    }
}