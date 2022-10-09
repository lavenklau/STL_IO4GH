using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using QuantumConcepts.Formats.StereoLithography;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace STLio
{
    public class STLInputComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public STLInputComponent()
          : base("STLinput", "stlIn",
              "Read STL file",
              "User", "STL")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("fileName", "fn", "path to a stl file", GH_ParamAccess.item);
            pManager.AddNumberParameter("weldAngle", "A", "angle to weld", GH_ParamAccess.item, 22.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "m", "readed stl mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            String fileName = "";
            double weldAngle = 22.5;
            bool suc1 = DA.GetData("fileName", ref fileName);
            bool suc2 = DA.GetData("weldAngle", ref weldAngle);
            if (suc1 && suc2)
            {
                try
                {
                    FileStream stlfile = new FileStream(fileName, FileMode.Open);
                    STLDocument doc = STLDocument.Read(stlfile);
                    Dictionary<Vertex, int> vid = new Dictionary<Vertex, int>();
                    Mesh m = new Mesh();
                    int vcounter = 0;
                    // add vertices and faces
                    foreach (Facet fac in doc) {
                        List<int> fvids = new List<int>();
                        foreach (Vertex v in fac) {
                            if (!vid.ContainsKey(v))
                            {
                                vid[v] = vcounter;
                                fvids.Add(vcounter);
                                vcounter++;
                                m.Vertices.Add(v.X, v.Y, v.Z);
                            }
                            else {
                                fvids.Add(vid[v]);
                            }
                        }
                        if (fvids.Count == 3)
                        {
                            m.Faces.AddFace(fvids[0], fvids[1], fvids[2]);
                            m.FaceNormals.AddFaceNormal(new Vector3d(fac.Normal.X, fac.Normal.Y, fac.Normal.Z));
                        }
                        else if (fvids.Count == 4)
                        {
                            m.Faces.AddFace(fvids[0], fvids[1], fvids[2], fvids[3]);
                            m.FaceNormals.AddFaceNormal(new Vector3d(fac.Normal.X, fac.Normal.Y, fac.Normal.Z));
                        }
                        else {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Non triangle or quad faces!!");
                        }
                    }

                    // weld vertices 
                    m.Weld(weldAngle / 180 * Math.PI); 

                    DA.SetData("mesh", m);
                }
                catch {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Exception occurred in mesh file reading");
                }
            }
            else {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "gather input parameter failed");
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                //return null;

                /// using System.Reflection!!!!
                return Properties.Resources.STL_IN.ToBitmap();
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e68088e4-003d-45bf-904e-8ed7af6e8a52"); }
        }
    }

    public class STLOutputComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public STLOutputComponent()
          : base("STLoutput", "stlOut",
              "Write STL file",
              "User", "STL")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "m", "mesh to be stored as STL format", GH_ParamAccess.item);
            pManager.AddTextParameter("fileName", "fn", "stl file to save to", GH_ParamAccess.item);
            pManager.AddBooleanParameter("binarySave", "b", "whether to same in a binary format", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("enable", "E", "enable this component to save", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("suc", "s", "finished save", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            String fileName = "";
            Mesh m = new Mesh();
            bool bin = false;
            bool en = false;
            bool suc1 = DA.GetData("fileName", ref fileName);
            bool suc2 = DA.GetData("binarySave", ref bin);
            bool suc3 = DA.GetData("mesh", ref m);
            DA.GetData("enable", ref en);
            if (suc1 && suc2 && suc3)
            {
                if (!en) {
                    DA.SetData("suc", en);
                    return;
                }
                try
                {
                    STLDocument doc = new STLDocument();
                    FileStream ofile = new FileStream(fileName, FileMode.Create);
                    if (!ofile.CanWrite) {
                        throw new Exception("file cannot write");
                    }
                    //bool hasNormal = m.FaceNormals.Count == m.Faces.Count;
                    for (int i = 0; i < m.Faces.Count; i++) {
                        var fn = m.FaceNormals[i];
                        Normal n = new Normal(fn.X, fn.Y, fn.Z);
                        List<Vertex> fv = new List<Vertex>();
                        var f = m.Faces[i];
                        var v = m.Vertices[f.A];
                        fv.Add(new Vertex(v.X, v.Y, v.Z));
                        v = m.Vertices[f.B];
                        fv.Add(new Vertex(v.X, v.Y, v.Z));
                        v = m.Vertices[f.C];
                        fv.Add(new Vertex(v.X, v.Y, v.Z));
                        if (f.IsQuad) {
                            v = m.Vertices[f.D];
                            fv.Add(new Vertex(v.X, v.Y, v.Z));
                        }
                        Facet fac = new Facet(n, fv, 0);
                        doc.Facets.Add(fac);
                    }
                    if (bin)
                    {
                        doc.WriteBinary(ofile);
                    }
                    else {
                        doc.WriteText(ofile);
                    }
                    DA.SetData("suc", true);
                    ofile.Close();
                }
                catch
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Exception occurred in mesh file writting");
                }
            }
            else {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "gather input parameter failed");
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                //return null;

                /// using System.Reflection!!!!
                return Properties.Resources.STL_OUT.ToBitmap();
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            // {7A85A71B-1A38-412E-A49C-62900A479270}
            get { return new Guid("7A85A71B-1A38-412E-A49C-62900A479270"); }
        }
    }
}
