using System;
using System.Collections.Generic;

namespace AirplaneManager.Models
{
    public class Airplane
    {
        private int _id;
        private string _model = string.Empty;
        private int _capacity;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string Model
        {
            get => _model;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Модель самолета не может быть пустой.");
                _model = value.Trim();
            }
        }

        public int Capacity
        {
            get => _capacity;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Вместимость самолета должна быть больше 0.");
                _capacity = value;
            }
        }


        public Airplane() { }

        public Airplane(int id, string model, int capacity)
        {
            Id = id;
            Model = model;
            Capacity = capacity;
        }

        public Airplane(string model, int capacity)
            : this(0, model, capacity) { }

        public override string ToString() => $"{Model} ({Capacity} мест)";
    }
}