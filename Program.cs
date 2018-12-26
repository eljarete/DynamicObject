using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;

namespace ObjetoDinamico
{
    public enum OpcionCadenaBusqueda
    {
        StartsWith,
        Contains,
        EndsWith
    }

    /// <summary>
    /// 
    /// </summary>
    class ReadOnlyFile : DynamicObject
    {
        // Almacena la ruta al fichero y el valor de la cuenta inicial de línea.
        private string p_filePath;
        
        /// <summary>
        /// Constructor público. Verifica que el fichero existe y aalmacena la ruta en la variable privada
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
        /// <param name="OpcionCadenaBusqueda">Permite escoger el tipo de búsqueda a realizar sobre cada línea (StartsWith, Contains, EndsWith).</param>
        /// <param name="trimSpaces">Permite escoger si realiza distinción entre mayúsculas o no (true, false).</param>
        /// <returns></returns>
        public List<string> GetPropertyValue(string nombrePropiedad,
                                     StringSearchOption OpcionCadenaBusqueda = OpcionCadenaBusqueda.StartsWith,
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

                    // Realiza una búsqueda sensible a mayúsculas usando las opciones de búsqueda especificadas.
                    testLine = line.ToUpper();
                    if (trimSpaces) testLine = testLine.Trim();

                    switch (StringSearchOption)                    {
                        case StringSearchOption.StartsWith:
                            if (testLine.StartsWith(nombrePropiedad.ToUpper())) { results.Add(line); }
                            break;
                        case StringSearchOption.Contains:
                            if (testLine.Contains(nombrePropiedad.ToUpper())) { results.Add(line); }
                            break;
                        case StringSearchOption.EndsWith:
                            if (testLine.EndsWith(nombrePropiedad.ToUpper())) { results.Add(line); }
                            break;
                    }
                }
            }
            catch
            {
                // Recoge cualquier excepción que ocurra durante la lectura del fichero y devuelve nulo.
                results = null;
            }
            finally
            {
                if (sr != null) { sr.Close(); }
            }
            return results;
        }
        
        /// <summary>
        /// Implementa el método TryGetMember de la clase DynamicObject para la llamada de miembros dinámicos.
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
        /// Implementa el método TryInvokeMember de la clase DynamicObject para llamadas de miembros dinámicos que tienen argumentos.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder,
                                             object[] args,
                                             out object result)
        {
            StringSearchOption StringSearchOption = StringSearchOption.StartsWith;
            bool trimSpaces = true;

            try
            {
                if (args.Length > 0) { StringSearchOption = (StringSearchOption)args[0]; }
            }
            catch
            {
                throw new ArgumentException("Los argumentos StringSearchOption deben ser un valor de enumerador StringSearchOption.");
            }

            try
            {
                if (args.Length > 1) { trimSpaces = (bool)args[1]; }
            }
            catch
            {
                throw new ArgumentException("El argumento trimSpaces debe ser un valor Booleano.");
            }

            result = GetPropertyValue(binder.Name, StringSearchOption, trimSpaces);

            return result == null ? false : true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            dynamic rFile = new ReadOnlyFile(@"..\..\TextFile1.txt");
            foreach (string line in rFile.Customer)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("----------------------------");
            foreach (string line in rFile.Customer(StringSearchOption.Contains, true))
            {
                Console.WriteLine(line);
            }
            Console.ReadKey();
        }
    }
}
