namespace SalonComunalApp.Services
{
  
    public abstract class ServicioBase
    {
        protected bool IdEsValido(int id)
        {
            return id > 0;
        }

        protected bool TextoEsValido(string texto)
        {
            return !string.IsNullOrWhiteSpace(texto);
        }
    }
}