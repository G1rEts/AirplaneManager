using System;

namespace AirplaneManager.Models
{
    public class Passenger : Person
    {
        private int _bonus;

        public int Bonus
        {
            get => _bonus;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Количество бонусов не может быть отрицательным.");
                _bonus = value;
            }
        }

        public Passenger() : base() { }

        public Passenger(int id, string firstName, string lastName, int age, string passport, int bonus)
            : base(id, firstName, lastName, age, passport)
        {
            Bonus = bonus;
        }

        // Для новых клиентов
        public Passenger(string firstName, string lastName, int age, string passport, int bonus)
            : base(firstName, lastName, age, passport)
        {
            Bonus = bonus;
        }
    }
}
