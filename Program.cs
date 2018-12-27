using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;

namespace ObjetoDinamico
{
    /// <summary>
    /// Criterios de b�squeda de cadena de caracteres.
    /// </summary>
    public enum OpcionCadenaBusqueda
    {
        StartsWith,
        Contains,
        EndsWith
    }

    /// <summary>
    /// Definici�n de fichero de s�lo lectura con par�metros din�micos.
    /// </summary>
    class ReadOnlyFile : DynamicObject
    {
        // Almacena la ruta al fichero y el valor de la cuenta inicial de l�nea.
        private string p_filePath;
        
        /// <summary>
        /// Constructor p�blico. Verifica que el fichero existe y aalmacena la ruta en la variable privada
        /// </summary>
        /// <param name="rutaFichero">Ruta del fichero</param>
        public ReadOnlyFile(string rutaFichero)        {
            if (!File.Exists(rutaFichero))  throw new Exception("La ruta del fichero no existe.");
            p_filePath = rutaFichero;
        }

        /// <summary>
        /// Obtiene una propiedad definidda en el fichero
        /// </summary>
        /// <param name="nombrePropiedad">Nombre de propiedad a buscar en el fichero.</param>
        /// <param name="OpcionCadenaBusqueda">Permite escoger el tipo de b�squeda a realizar sobre cada l�nea (StartsWith, Contains, EndsWith).</param>
        /// <param name="trimSpaces">Permite escoger si realiza distinci�n entre may�sculas o no (true, false).</param>
        /// <returns></returns>
        public List<string> GetPropertyValue(string nombrePropiedad,
                                     OpcionCadenaBusqueda OpcionCadenaBusqueda = OpcionCadenaBusqueda.StartsWith,
                                     bool trimSpaces = true)
        {
            StreamReader sr = null;
            List<string> results = new List<string>();
            string line = "";
            string testLine = "";

            try
            {
                sr = new StreamReader(p_filePath);

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();

                    // Realiza una b�squeda sensible a may�sculas usando las opciones de b�squeda especificadas.
                    testLine = line.ToUpper();
                    if (trimSpaces) testLine = testLine.Trim();

                    switch (OpcionCadenaBusqueda)                    {
                        case OpcionCadenaBusqueda.StartsWith:
                            if (testLine.StartsWith(nombrePropiedad.ToUpper())) { results.Add(line); }
                            break;
                        case OpcionCadenaBusqueda.Contains:
                            if (testLine.Contains(nombrePropiedad.ToUpper())) { results.Add(line); }
                            break;
                        case OpcionCadenaBusqueda.EndsWith:
                            if (testLine.EndsWith(nombrePropiedad.ToUpper())) { results.Add(line); }
                            break;
                    }
                }
            }
            catch
            {
                // Recoge cualquier excepci�n que ocurra durante la lectura del fichero y devuelve nulo.
                results = null;
            }
            finally
            {
                if (sr != null) { sr.Close(); }
            }
            return results;
        }
        
        /// <summary>
        /// Implementa el m�todo TryGetMember de la clase DynamicObject para la llamada de miembros din�micos.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder,
                                          out object result)
        {
            result = GetPropertyValue(binder.Name);
            return result == null ? false : true;
        }

        /// <summary>
        /// Implementa el m�todo TryInvokeMember de la clase DynamicObject para llamadas de miembros din�micos que tienen argumentos.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder,
                                             object[] args,
                                             out object result)
        {
            OpcionCadenaBusqueda OpcionCadenaBusqueda = OpcionCadenaBusqueda.StartsWith;
            bool trimSpaces = true;

            try
            {
                if (args.Length > 0) { OpcionCadenaBusqueda = (OpcionCadenaBusqueda)args[0]; }
            }
            catch
            {
                throw new ArgumentException("Los argumentos OpcionCadenaBusqueda deben ser un valor de enumerador OpcionCadenaBusqueda.");
            }

            try
            {
                if (args.Length > 1) { trimSpaces = (bool)args[1]; }
            }
            catch
            {
                throw new ArgumentException("El argumento trimSpaces debe ser un valor Booleano.");
            }

            result = GetPropertyValue(binder.Name, OpcionCadenaBusqueda, trimSpaces);

            return result == null ? false : true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            dynamic rFile = new ReadOnlyFile(@"..\..\TextFile1.txt");
            foreach (string line in rFile.Supplier)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("----------------------------");
            foreach (string line in rFile.Customer(OpcionCadenaBusqueda.Contains, true))
            {
                Console.WriteLine(line);
            }
            Console.ReadKey();
        }
    }
}
