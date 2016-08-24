using Ada.Framework.Expressions;
using Ada.Framework.Expressions.Entities;
using Ada.Framework.Maintenance.FileManager.Entities;
using Ada.Framework.Util.FileMonitor;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ada.Framework.Maintenance.FileManager.Console
{
    public class Program
    {
        static int Main(string[] args)
        {
            IMonitorArchivo monitor = MonitorArchivoFactory.ObtenerArchivo();
            string rutaLogError = monitor.ObtenerRutaAbsoluta(".[DS]Error.log.txt");

            try
            {
                FileSystemManager manager = new FileSystemManager();
                AdministradorArchivoTag admin = manager.ObtenerConfiguracion();

                foreach (Accion accion in admin.Acciones)
                {
                    foreach (FileSystem fileSystem in FileSystemManager.ObtenerFileSystems(accion))
                    {
                        IList<Evaluador<object>> evaluadores = FileSystemManager.ObtenerEvaluadores(admin.Evaluadores);
                        IList<ExpresionCondicional> condiciones = FileSystemManager.ObtenerCondiciones(accion.Condiciones);

                        if (EvaluadorExpresiones.EvaluarCondiciones(fileSystem, condiciones, evaluadores))
                        {
                            accion.Realizar(fileSystem);
                        }
                    }
                }

                if(monitor.Existe(rutaLogError))
                {
                    new Eliminar().Realizar(new FileSystem() { Info = new FileInfo(rutaLogError), Tipo = TipoFileSystem.Archivo });
                }
            }
            catch (Exception ex)
            {
                FileStream logError = new FileStream(rutaLogError, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter writer = new StreamWriter(logError);
                writer.AutoFlush = true;

                Exception exepcionAux = ex;
                while (exepcionAux != null)
                {
                    writer.WriteLine(exepcionAux.Message);
                    writer.WriteLine("------------------------------------------------------------------------------");
                    writer.WriteLine(exepcionAux.StackTrace);
                    writer.WriteLine("\n");
                    exepcionAux = exepcionAux.InnerException;
                }

                writer.Close();
                logError.Close();
                return 1;
            }
            return 0;
        }
    }
}