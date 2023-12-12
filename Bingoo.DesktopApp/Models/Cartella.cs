using System;
using System.Collections.Generic;

namespace Bingoo.Models
{
    internal class Cartella
    {
        public string Id { get; set; }
        public List<string> FirstRow { get; set;} = new();
        public List<string> SecondRow { get; set; } = new();
        public List<string> ThirdRow { get; set; } = new();

        public void ValidateOrThrowException()
        {
            _ = Id ?? throw new Exception("L'id è obbligatorio.");

            if (!FirstRow?.TrueForAll(number =>
            {
                _ = int.TryParse(number, out var tempNumber);
                return tempNumber > 0 && tempNumber <= 90;
            }) ?? true)
                throw new Exception("La prima riga contiene numeri non validi.");

            if (!SecondRow?.TrueForAll(number =>
            {
                _ = int.TryParse(number, out var tempNumber);
                return tempNumber > 0 && tempNumber <= 90;
            }) ?? true)
                throw new Exception("La seconda riga contiene numeri non validi.");

            if (!ThirdRow?.TrueForAll(number =>
            {
                _ = int.TryParse(number, out var tempNumber);
                return tempNumber > 0 && tempNumber <= 90;
            }) ?? true)
                throw new Exception("La terza riga contiene numeri non validi.");
        }
    }
}
