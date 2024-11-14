using System.Collections.Generic;
using System.Linq;

namespace PIA.Model
{
    public class Receta
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Instrucciones { get; set; }
        public bool Publica { get; set; }
        public string ImageUrl { get; set; } 

        public List<string> Ingredientes { get; set; } = new List<string>();

        public Receta() { }

        public bool EsValida()
        {
            return !string.IsNullOrWhiteSpace(Nombre) &&
                   !string.IsNullOrWhiteSpace(Instrucciones);
        }

        public string PreviewIngredientes
        {
            get
            {
                if (Ingredientes == null || !Ingredientes.Any())
                    return string.Empty;

                var preview = string.Join(", ", Ingredientes.Take(3));
                return Ingredientes.Count > 3 ? $"{preview}..." : preview;
            }
        }

        public string IngredientesText
        {
            get => string.Join(", ", Ingredientes);
            set
            {
                Ingredientes = value.Split(',').Select(i => i.Trim()).ToList();
            }
        }

    }
}
