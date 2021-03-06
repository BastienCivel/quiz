using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;


/*Importation des Librairies Kinect / Microsoft*/
using Microsoft.Kinect;
using System.Threading;

namespace Kinect_firs
{
    class Program
    {
        //Recuperation des Fonctions Kinect
        private static KinectSensor kinectSensor;
        private static BodyFrameReader bodyFrameReader = null;
        private static Body[] bodies = null;
        private static CoordinateMapper coordinateMapper = null;
        private const float InferredZPositionClamp = 0.1f;
        
        
        //Récupérer réponses et questions
        private string[] sQuestion=;
        private string[] sReponses;
        private int indexBonneReponse;
        
        // Points du corps
        static JointType HandRight;
        static JointType MidSpine;
        static float RHand_X;
        static float RHand_Y;
        static float MSpine_X;
        static float MSpine_Y;

        static void Main(string[] args)
        {
            /**************************************************************/
            /////////////////       GESTION GET            ///////////////// 
            /**************************************************************/
            string sURL;
            sURL = "http://13.95.14.230:82/quiz";
            JsonObject j;

            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(sURL);

            Stream objStream;
            objStream = wrGETURL.GetResponse().GetResponseStream();

            StreamReader objReader = new StreamReader(objStream);

            string sLine = "";
            string sJsonTotal ="";

            while (sLine!=null)
            {
            i++;
            sLine = objReader.ReadLine();
            if (sLine!=null)
           //Console.WriteLine("{0}:{1}",i,sLine);
                sJsonTotal+=sLine;                            
            }
            
            //Appel à la fonction de Parser
            JsonToObject(sJsonTotal);
            
             /**************************************************************/
            /////////////////       GESTION GET            ///////////////// 
            /**************************************************************/
            
                        
            
            //Lancement du premier Capteur
            kinectSensor = KinectSensor.GetDefault();
            coordinateMapper = kinectSensor.CoordinateMapper;
            //Ouverture du premier capteur*
            kinectSensor.Open();

            //Lancement de la recherche d'un corps
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            if (bodyFrameReader != null)
            {
                //Tentative d'acces
                try
                {
                    bodyFrameReader.FrameArrived += Reader_FrameArrived;
                }
                //Verification d'une exception dans la recherche
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            //En cas de recherche infructueuse > on arrete 
            else
            {
                Console.WriteLine("BodyFrameReader is null");
            }
            Console.ReadLine();
        }
                    
            
             /**************************************************************/
            /////////////////       GESTION POST            ///////////////// 
            /**************************************************************/
                 
            /// <summary>
            /// Cette fonction permet d'appelle une requête POST à la BDD
            /// Pour le moment, il faut récupérer l'ID du quiz via la méthode GET, mais ne pouvant tester, je le mets comme ça. 
            /// Cette fonction est appellée lors de la réception d'une frame, et renvoie donc la zone de réponse dans laquelle elle se trouvve
            /// </summary>
            /// <param name="sZoneReponse">Numéro de la réponse donnée</param>
            protected  void PostRequest(String sZoneReponse)
            {
                
            //IL FAUDRA ICI RENSEIGNER ID DU QUIZ (RECUP VIA GET REQUEST)
            WebRequest request = WebRequest.Create("http://13.95.14.230:82/quiz/participation/");
            request.Method = "POST";

            // Créer le POST et le converti en byte array
            string postData = sZoneReponse;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "text/json";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();

            // 0 correspond à l'index du flux de données (stream)
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            
            // Obtient le contenu du flux retourné par le serveur
            dataStream = response.GetResponseStream();

            // On close les Streams
            reader.Close();
            dataStream.Close();
            response.Close();
            }
             /**************************************************************/
            /////////////////       GESTION POST            ///////////////// 
            
            /**************************************************************/
            
             /**************************************************************/
            /////////////////       GESTION PARSER            ///////////////// 
            /**************************************************************/
            /// <summary>
            /// Cette fonction permet d'appelle de parser le Json que l'on reçoit et le transformer en un "object"
            /// A partir de cet object on peut récupérer les questions et les réponses 
            /// Les itérations des boucles proviennent du nombre de questions/réponses présentes dans le fichier quiz-sample disponible à la racine du projet.
            /// </summary>
            /// <param name="JsonData">Le JSON renvoyé par la requête GET</param>
            public static void JsonToObject(string JsonData)
            {
                //Transformer le Json en un object
                JavaScriptSerializer jss = new JavaScriptSerializer();
                var JsonObject = jss.Deserialize<dynamic>(JsonData);
                
                //Récupérer les questions
                int j=0;
                for (j=0; j<2;j++)
                {
                sQuestion[] = new string[];
                sQuestion[j] = JsonObject.questions[j].["title"];
                }
                
                //Récupérer les réponses
                int i =0;
                for (j=0;j<2;j++)
                {
                    for (i=0; i<3;i++)
                    {
                        sReponses[] = new string[];
                        sReponses[i] = JsonObject.questions[j].answers[i].["title"];
                   }                   
                }
            
            }
            
             /**************************************************************/
            /////////////////       GESTION PARSER            ///////////////// 
            /**************************************************************/
    

        // Verifier la zone de la main droite
        static String checkZoneRH()
        {
            String zoneHand;
            if (RHand_X < MSpine_X && RHand_Y < MSpine_Y)
            {
                zoneHand = "Zone 1";
                PostRequest(zoneHand);
            }
            else if (RHand_X > MSpine_X && RHand_Y < MSpine_Y)
            {  
                zoneHand = "Zone 2";
                PostRequest(zoneHand);
            }
            else if (RHand_X < MSpine_X && RHand_Y > MSpine_Y)
            {  
                zoneHand = "Zone 3";
                PostRequest(zoneHand);
            }
            else if (RHand_X > MSpine_X && RHand_Y > MSpine_Y)
            {  
                zoneHand = "Zone 4";
                PostRequest(zoneHand);
            }
            else
                zoneHand = "Hand not found";
                

            return zoneHand;
        }
        
    

        // Lecture du capteur
        static void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            // Test de réception de données
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                        bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }
            // Si des données sont reçus
            if (dataReceived)
            {
                // Pour chaques partie du corps
                foreach (Body body in bodies)
                {
                    if (body.IsTracked)
                    {
                        /*Recuperation des fonctions de detection des parties du corps humain dans la librairies Kinect*/
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                        /*Recuperation des Elements Main Gauche, Main Gauche et Tete*/
                        JointType HandRight = (Microsoft.Kinect.JointType)11;
                        JointType Head = (Microsoft.Kinect.JointType)3;

                        // Pour chaques points du corps tracker
                        foreach (JointType jointType in joints.Keys)
                        {
                            // Heure de tacking
                            String hnow = DateTime.Now.ToString("mm:ss");

                            /*Tentative de Detection de la Main Droite  */
                            CameraSpacePoint positionHR = joints[HandRight].Position;
                            if (positionHR.Z < 0)
                            {
                                positionHR.Z = InferredZPositionClamp;
                            }

                            /*Recuperation des Coordonnées X,Y de la main Droite*/
                            DepthSpacePoint depthSpacePoint2 = coordinateMapper.MapCameraPointToDepthSpace(positionHR);
                            RHand_X = depthSpacePoint2.X;
                            RHand_Y = depthSpacePoint2.Y;

                            // Milieu de la colonne
                            CameraSpacePoint positionMS = joints[MidSpine].Position;
                            if (positionMS.Z < 0)
                            {
                                positionMS.Z = InferredZPositionClamp;
                            }
                            // Recherche des coordonées du milieu de la colonne
                            DepthSpacePoint depthSpacePoint3 = coordinateMapper.MapCameraPointToDepthSpace(positionMS);
                            MSpine_X = depthSpacePoint3.X;
                            MSpine_Y = depthSpacePoint3.Y;
                            String zoneMain = checkZoneRH();
                            Console.WriteLine(zoneMain);
                        }

                        /*Pause pour laisser le temps au systeme d'afficher la donnée*/
                        int milliseconds = 3000;
                        Thread.Sleep(milliseconds);
                    }
                    /*Annonce qu'aucun corps n'a été trouvé*/
                    else
                    {
                        //Console.WriteLine("Body Not TRack");
                    }
                }
            }

            /*Confirmation Ecrite qu'aucune donnée n'ait été recu*/
            else
            {
                Console.WriteLine("Data NOT Received");
            }
        }
    }
}