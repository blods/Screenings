using System;
using System.Web;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.WebPartPages;

namespace ScreeningsWebPart
{
   

    public class Screenings : System.Web.UI.WebControls.WebParts.WebPart
    {


        public Screenings()
        {
        }
        private Microsoft.SharePoint.SPWeb _web = null;
        private Microsoft.SharePoint.SPList _screenings = null;
        private Microsoft.SharePoint.SPList _SignUp = null;

        protected Label lblValidationError;
        protected Button btSignUp;
        protected Button btRemove;
        protected DropDownList ddAttending;



        protected override void CreateChildControls()
        {

            lblValidationError = new Label();
            lblValidationError.CssClass = "ms-formvalidation";


            this._web = Microsoft.SharePoint.SPContext.Current.Web;
            this._screenings = this._web.Lists["Screenings"];
            this._SignUp = this._web.Lists["Sign ups"];
            Guid guidViewID = this._SignUp.Views["Upcoming Screenings"].ID;
            SPView oViewSrc = this._SignUp.GetView(guidViewID);
            SPListItemCollection collItemsSrc = this._SignUp.GetItems(oViewSrc);

            foreach (SPListItem o in collItemsSrc)
            {

                btRemove = new Button();
                btRemove.Text = "Remove";
                btRemove.Command += new CommandEventHandler(Remove_click);
                btRemove.CommandName = o["ID"].ToString();
                btRemove.ID = "btRemove" + o["ID"].ToString();
                Controls.Add(btRemove);
            }

            foreach (SPListItem i in this._screenings.Items)
            {
                int limit = Attend(int.Parse(i["ID"].ToString()));

                if (limit == 0)
                {
                    btSignUp = new Button();
                    btSignUp.Text = "Sign up";
                    btSignUp.Command += new CommandEventHandler(saveTitle_click);
                    btSignUp.CommandName = i["ID"].ToString();
                    btSignUp.ID = "btSignUp" + i["ID"].ToString();
                    btSignUp.Enabled = false;

                    Controls.Add(btSignUp);

                    ddAttending = new DropDownList();
                    ddAttending.ID = "ddAttending" + i["ID"].ToString();
                    ddAttending.Items.Add("0");


                    Controls.Add(ddAttending);
                }
                else
                {
                    btSignUp = new Button();
                    btSignUp.Text = "Sign up";
                    btSignUp.Command += new CommandEventHandler(saveTitle_click);
                    btSignUp.CommandName = i["ID"].ToString();
                    btSignUp.ID = "btSignUp" + i["ID"].ToString();

                    Controls.Add(btSignUp);

                    ddAttending = new DropDownList();
                    ddAttending.ID = "ddAttending" + i["ID"].ToString();


                    for (int a = 1; a < limit + 1; a++)
                    {
                        ddAttending.Items.Add(a.ToString());

                    }
                    Controls.Add(ddAttending);
                }
            }
        }

        private int Attend(int id)
        {

            this._web = Microsoft.SharePoint.SPContext.Current.Web;
            this._SignUp = this._web.Lists["Sign Ups"];
            this._screenings = this._web.Lists["Screenings"];
            int fullName = this._web.CurrentUser.ID;
            SPListItem o = this._screenings.Items.GetItemById(id);

            int Limit = int.Parse(o["Guest Limit"].ToString());
            int attending = 0;

            SPQuery oQuery = new SPQuery();
            oQuery.Query =
                "<Where>" +
                "<And>" +
               "<Eq><FieldRef Name='Employee' LookupId='TRUE'/><Value Type='User'>" + fullName + "</Value></Eq>" +
               "<Eq><FieldRef Name='Screening_x0020_ID'/><Value Type='Number'>" + id + "</Value></Eq>" +
                "</And>" +
                "</Where>";
            SPListItemCollection coll = this._SignUp.GetItems(oQuery);

            foreach (SPListItem item in coll)
            {
                attending = attending + int.Parse(item["Attending"].ToString());
            }

            if (int.Parse(o["Max Attending"].ToString()) - int.Parse(o["Current Attending"].ToString()) < Limit)
            {
                Limit = int.Parse(o["Max Attending"].ToString()) - int.Parse(o["Current Attending"].ToString());
            }

            if (attending < Limit)
            {
                return Limit - attending;
            }
            else
            {
                return 0;
            }

        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            this._web = Microsoft.SharePoint.SPContext.Current.Web;
            this._screenings = this._web.Lists["Screenings"];
            this._SignUp = this._web.Lists["Sign ups"];
            Guid guidViewID = this._SignUp.Views["Upcoming Screenings"].ID;
            SPView oViewSrc = this._SignUp.GetView(guidViewID);
            SPListItemCollection collItemsSrc = this._SignUp.GetItems(oViewSrc);

            writer.Write("<table width=100%>");
            writer.Write("<tr>");
            writer.Write("<td colspan=6>");
            lblValidationError.RenderControl(writer);
            writer.Write("</td>");
            writer.Write("</tr>");
            writer.Write("<tr>");
            writer.Write("<td>Screening</td>");
            writer.Write("<td>Date</td>");
            writer.Write("<td>Start</td>");
            writer.Write("<td>Attending</td>");
            writer.Write("<td># to Attend</td>");
            writer.Write("<td>Sign up</td>");
            writer.Write("</tr>");

            foreach (SPListItem i in this._screenings.Items)
            {
                writer.Write("<tr>");
                writer.Write("<td>");
                writer.Write(i["Title"].ToString());
                writer.Write("</td>");
                writer.Write("<td>");
                writer.Write(String.Format("{0:D}", i["Start"]).ToString());
                writer.Write("</td>");
                writer.Write("<td>");
                writer.Write(String.Format("{0:t}", i["Start"]).ToString());
                writer.Write("</td>");
                writer.Write("<td>");
                writer.Write(i["Current Attending"] + " of " + i["Max Attending"]);
                writer.Write("</td>");
                writer.Write("<td>");

                DropDownList dd = (DropDownList)FindControl("ddAttending" + i["ID"].ToString());
                dd.RenderControl(writer);
                writer.Write("</td>");
                writer.Write("<td>");


                Button c = (Button)FindControl("btSignUp" + i["ID"].ToString());

                c.RenderControl(writer);

                writer.Write("</td>");
                writer.Write("</tr>");

            }
            writer.Write("<tr Hieght=30>");
            writer.Write("<td colspan=6>");
            writer.Write("</td>");
            writer.Write("</tr>");
            writer.Write("<tr >");
            writer.Write("<td colspan=6><h3 class='ms-standardheader ms-WPTitle'>Screenings I'm Attending</h3>");
            writer.Write("</td>");
            writer.Write("</tr>");
            writer.Write("<tr>");
            writer.Write("<td>Screening</td>");
            writer.Write("<td>Date</td>");
            writer.Write("<td>Start</td>");
            writer.Write("<td>Attending</td>");
            writer.Write("<td colspan=2>Remove</td>");
            writer.Write("</tr>");

            foreach (SPListItem o in collItemsSrc)
            {
                writer.Write("<tr>");
                writer.Write("<td>");
                writer.Write(o[0]);
                writer.Write("</td>");
                writer.Write("<td>");
                writer.Write(String.Format("{0:D}", o["Start"]).ToString());
                writer.Write("</td>");
                writer.Write("<td>");
                writer.Write(String.Format("{0:t}", o["Start"]).ToString());
                writer.Write("</td>");
                writer.Write("<td>");
                writer.Write(o["Attending"]);
                writer.Write("</td>");
                writer.Write("<td colspan=2>");



                Button r = (Button)FindControl("btRemove" + o["ID"].ToString());

                r.RenderControl(writer);

                writer.Write("</td>");
                writer.Write("</tr>");
            }

            writer.Write("</table>");



        }



        public void Remove_click(object sender, CommandEventArgs e)
        {
            Guid siteid = SPContext.Current.Site.ID;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteid))
                {
                    //SPSite site = new SPSite(siteid);
                    //SPWeb web = site.OpenWeb("/Screenings");
                    this._web = site.OpenWeb(Microsoft.SharePoint.SPContext.Current.Web.ID);
                    this._web.AllowUnsafeUpdates = true;
                    this._screenings = this._web.Lists["Screenings"];
                    this._SignUp = this._web.Lists["Sign ups"];

                    //SPList SignUp = web.Lists["Sign Ups"];

                    //SPList screenings = web.Lists["Screenings"];
                    int att = 0;
                    int scrnID = 0;
                    int id = int.Parse(e.CommandName);

                    SPListItem o = this._SignUp.Items.GetItemById(id);
                    att = int.Parse(o["Attending"].ToString());
                    scrnID = int.Parse(o["Screening ID"].ToString());


                    SPListItem s = this._screenings.Items.GetItemById(scrnID);

                    s["Current Attending"] = int.Parse(s["Current Attending"].ToString()) - att;
                    s.Update();


                    this._screenings.Update();
                    o.Web.AllowUnsafeUpdates = true;

                    o.Delete();
                    this._SignUp.Update();

                    Controls.Clear();
                    CreateChildControls();


                }

            });
        }
        //   {
        //       Guid siteid = SPContext.Current.Site.ID;

        //       SPSecurity.RunWithElevatedPrivileges(delegate()
        //{
        //    //SPSite site = new SPSite(siteid);
        //   //SPWeb web = site.OpenWeb("/Screenings");
        //    this._web = Microsoft.SharePoint.SPContext.Current.Web;
        //    this._web.AllowUnsafeUpdates = true;
        //    this._screenings = this._web.Lists["Screenings"];
        //    this._SignUp = this._web.Lists["Sign ups"];

        //   //SPList SignUp = web.Lists["Sign Ups"];

        //    //SPList screenings = web.Lists["Screenings"];
        //    int att = 0;
        //    int scrnID = 0;
        //  int id = int.Parse(e.CommandName);

        //  SPListItem o = SignUp.Items.GetItemById(id);
        //  att = int.Parse(o["Attending"].ToString());
        //  scrnID = int.Parse(o["Screening ID"].ToString());


        //   SPListItem s = screenings.Items.GetItemById(scrnID);

        //   s["Current Attending"] = int.Parse(s["Current Attending"].ToString()) - att;
        //   s.Update();


        //   screenings.Update();
        //   o.Web.AllowUnsafeUpdates = true;

        //   o.Delete();
        //   SignUp.Update();

        //   Controls.Clear();
        //   CreateChildControls();




        //});
        //   }


        public void saveTitle_click(object sender, CommandEventArgs e)
        {
            Guid siteid = Microsoft.SharePoint.SPContext.Current.Site.ID;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteid))
                {
                    this._web = site.OpenWeb(Microsoft.SharePoint.SPContext.Current.Web.ID);
                    //this._web.AllowUnsafeUpdates = true;
                    this._web.AllowUnsafeUpdates = true;
                    this._screenings = this._web.Lists["Screenings"];
                    this._SignUp = this._web.Lists["Sign ups"];

                    Int16 id = Convert.ToInt16(e.CommandName);

                    SPListItem o = this._screenings.Items.GetItemById(id);

                    SPListItem myNewItem = this._SignUp.Items.Add();
                    myNewItem.Web.AllowUnsafeUpdates = true;
                    DropDownList ct1 = (DropDownList)FindControl("ddAttending" + id.ToString());

                    if (int.Parse(o["Current Attending"].ToString()) + int.Parse(ct1.SelectedValue.ToString()) <= int.Parse(o["Max Attending"].ToString()))
                    {

                        myNewItem["Title"] = o["Title"];
                        myNewItem["Start"] = o["Start"];
                        myNewItem["Employee"] = SPContext.Current.Web.CurrentUser.ID.ToString() + ";#" + SPContext.Current.Web.CurrentUser.LoginName;

                        myNewItem["Attending"] = ct1.SelectedValue;
                        myNewItem["Screening ID"] = id;
                        myNewItem.Update();
                        this._SignUp.Update();
                        o["Current Attending"] = int.Parse(o["Current Attending"].ToString()) + int.Parse(ct1.SelectedValue.ToString());
                        o.Update();
                        this._screenings.Update();
                        Controls.Clear();
                        CreateChildControls();

                    }
                    else
                    {
                        lblValidationError.Text = "Sorry but the screening is full";
                        lblValidationError.Visible = true;
                    }
                }
            });

        }
        //{
        //    Guid siteid = Microsoft.SharePoint.SPContext.Current.Site.ID;
        //    SPSecurity.RunWithElevatedPrivileges(delegate()
        //    {
        //        SPSite site = new SPSite(siteid);
        //        SPWeb web = site.OpenWeb("/Screenings");
        //        web.AllowUnsafeUpdates = true;
        //        SPList SignUp = web.Lists["Sign Ups"];
        //        SPList screenings = web.Lists["Screenings"];

        //        Int16 id = Convert.ToInt16(e.CommandName);

        //        SPListItem o = screenings.Items.GetItemById(id);

        //        SPListItem myNewItem = SignUp.Items.Add();
        //        myNewItem.Web.AllowUnsafeUpdates = true;
        //        DropDownList ct1 = (DropDownList)FindControl("ddAttending" + id.ToString());

        //        if (int.Parse(o["Current Attending"].ToString()) + int.Parse(ct1.SelectedValue.ToString()) <= int.Parse(o["Max Attending"].ToString()))
        //        {

        //            myNewItem["Title"] = o["Title"];
        //            myNewItem["Start"] = o["Start"];
        //            myNewItem["Employee"] = this._web.CurrentUser.ID.ToString() + ";#" + this._web.CurrentUser.LoginName;
        //            myNewItem["Attending"] = ct1.SelectedValue;
        //            myNewItem["Screening ID"] = id;
        //            myNewItem.Update();
        //            SignUp.Update();
        //            o["Current Attending"] = int.Parse(o["Current Attending"].ToString()) + int.Parse(ct1.SelectedValue.ToString());
        //            o.Update();
        //            screenings.Update();
        //            Controls.Clear();
        //            CreateChildControls();

        //        }
        //        else
        //        {
        //            lblValidationError.Text = "Sorry but the screening is full";
        //            lblValidationError.Visible = true;
        //        }

        //    });

        //}
    }


}
