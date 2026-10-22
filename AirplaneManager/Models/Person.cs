using System;
using System.Linq;

namespace AirplaneManager.Models
{
    public abstract class Person
    {
        private int _id;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private int _age;
        private string _passport = string.Empty;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Имя не может быть пустым.");

                if (!value.Trim().All(char.IsLetter))
                    throw new ArgumentException("Имя может содержать только буквенные символы.");

                _firstName = value.Trim();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Фамилия не может быть пустой.");

                if (!value.Trim().All(char.IsLetter))
                    throw new ArgumentException("Фамилия может содержать только буквенные символы.");

                _lastName = value.Trim();
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (value < 0 || value > 120)
                    throw new ArgumentOutOfRangeException(nameof(value), "Возраст должен быть в диапазоне от 0 до 120 лет.");
                _age = value;
            }
        }

        public string Passport
        {
            get => _passport;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Номер паспорта не может быть пустым.");

                string trimmed = value.Trim();

                // Проверяем формат через простую маску: 4 цифры + пробел + 6 цифр
                if (trimmed.Length != 11 || trimmed[4] != ' ' || !long.TryParse(trimmed.Replace(" ", ""), out _))
                    throw new ArgumentException("Паспорт должен быть в формате: 1234 567890 (4 цифры, пробел, 6 цифр).");

                _passport = trimmed;
            }
        }

        protected Person() { }

        protected Person(int id, string firstName, string lastName, int age, string passport)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Passport = passport;
        }

        protected Person(string firstName, string lastName, int age, string passport)
            : this(0, firstName, lastName, age, passport) { }
    }
}