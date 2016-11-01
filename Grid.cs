using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Grid.Helpers
{
    public struct ForeignKeyParameter
    {
        public string TableName;
        public string FieldName;
        public Type ModelType;
    }

    public struct Hyperlink
    {
        public string FieldName;
        public string URL;
    }

    public struct GridAction
    {
        public string Text;
        public string URL;
    }

    public class Grid<T>
    {
        private string HTML;
        private string Header;
        private string Body;

        public List<T> Model;
        public string ControllerName;
        public string PrimaryKey;
        public string[] Fields;
        public int MarginBottom = 20;
        public bool ShowActions = true;
        public bool ShortHeader = false;
        public bool ShowHeader = true;
        public bool ShowPrimaryKey = true;
        public bool ShowDefaultActions = true;
        public bool ShowPager = true;
        public bool ShowCheckBox = false;
        public Hyperlink Hyperlinks;
        public List<ForeignKeyParameter> ForeignKeys;
        public List<GridAction> CustomActions;
        public int LimitNumberRows = 0;
        public int LimitNumberPages = 0;
        public string PagerCustomLink = null;

        public Grid()
        {
            HTML = @"<div class=""table-responsive"">
                <table class=""table table-striped table-hover margin-bottom-{MARGINBOTTOM}"">
                <thead>
                    <tr>{HEADER}</tr>
                </thead>
                <tbody>
                    <tr>{BODY}</tr>
                </tbody>
                </table>
                {PAGER}
                </div>";
        }

        public string Compile()
        {
            if (Model != null)
            {
                // add the PrimaryKey to the list of fields
                if (!Fields.Contains(PrimaryKey)) this.UpdateFields();

                // add the pager if there is a limitation
                if (LimitNumberRows > 0)
                    this.MakePager();
                else
                    HTML = HTML.Replace("{PAGER}", "");

                // add or remove the header
                if (ShowHeader == true)
                    MakeHeader();
                else
                    RemoveHeader();

                // add margin-bottom
                HTML = HTML.Replace("{MARGINBOTTOM}", MarginBottom.ToString());

                MakeBody();
                return HTML;
            }
            else
            {
                return "";
            }
        }

        private void MakeHeader()
        {
            var properties = typeof(T).GetProperties();

            if (ShowCheckBox == true)
            {
                Header += "<th>#</th>";
            }

            foreach (var property in properties)
            {
                if (Fields.Contains(property.Name))
                {
                    if (property.Name == PrimaryKey)
                    {
                        // show the primary key header if it's enable
                        if (ShowPrimaryKey == true)
                            Header += "<th>#</th>";
                    }
                    else
                    {
                        if (ShortHeader == true)
                        {
                            // add spapce beetween words 
                            string result = Regex.Replace(property.Name, @"(\p{Lu})", " $1").TrimStart();
                            // find the index of secoend word
                            int index = result.IndexOf(' ');
                            //remove the frist word
                            if (index > 0)
                            {
                                result = result.Remove(0, index);
                            }
                            Header += "<th>" + result + "</th>";
                        }
                        else
                        {
                            Header += "<th>" + property.Name + "</th>";
                        }
                    }
                }
            }

            if (ShowActions == true)
            {
                Header += "<th>Actions</th>";
            }

            HTML = HTML.Replace("{HEADER}", Header);
        }

        private void RemoveHeader()
        {
            HTML = HTML.Replace("{HEADER}", "");
        }

        private void MakeBody()
        {
            var properties = typeof(T).GetProperties();

            // go through the given Model 
            foreach (var item in Model)
            {
                int ID = 0;
                Body += "<tr>";

                // go through the properties of the Model
                foreach (var property in properties)
                {
                    if (Fields.Contains(property.Name))
                    {
                        // does it have ForeignKeys?
                        if (GetForeignKey(property.Name).FieldName != null)
                        {
                            // get the parameters of the foreign key
                            ForeignKeyParameter FKP = GetForeignKey(property.Name);
                            // get the object of the foreign key
                            var FKObject = property.GetValue(item, null);
                            // find the type of the object
                            Type type = FKP.ModelType;
                            // obtain the value via FieldName and FKObject
                            var value = type.GetProperty(FKP.FieldName).GetValue(FKObject, null);
                            Body += string.Format("<td>{0}</td>", value);
                        }
                        // doesn't have ForeignKeys
                        else
                        {
                            // check if the property is the primary key
                            if (property.Name == PrimaryKey)
                            {
                                ID = int.Parse(property.GetValue(item, null).ToString());
                                if (ShowCheckBox == true)
                                {
                                    Body += "<th><input type='checkbox' name='Selected" + typeof(T).Name + "' value='" + ID + "'></th>";
                                }
                                if (ShowPrimaryKey == true)
                                {
                                    // add the the primary key to the body
                                    var value = property.GetValue(item, null);
                                    this.MakeField(property.Name, value, ID);
                                }
                            }
                            else
                            {
                                // add the field to the body
                                var value = property.GetValue(item, null);
                                this.MakeField(property.Name, value, ID);
                            }
                        }
                    }
                }

                // check if there is any actions
                if (ShowActions == true && ID != 0)
                {
                    MakeActionsBody(ID);
                }

            }

            HTML = HTML.Replace("{BODY}", Body);
        }

        private void MakeActionsBody(int ID)
        {
            string act = "<td>";
            if (ShowDefaultActions == true)
            {
                act += @"<a href=""/{0}/Edit/{1}"" class=""btn btn-sm btn-icon btn-pure btn-default"" data-toggle=""tooltip"" data-original-title=""Edit"">
                         <i class=""icon wb-wrench"" aria-hidden=""true""></i></a>
                         <a href=""/{0}/Delete/{1}"" class=""btn btn-sm btn-icon btn-pure btn-default"" data-toggle=""tooltip"" data-original-title=""Delete"">
                         <i class=""icon wb-close"" aria-hidden=""true""></i></a>";
            }
            else if (CustomActions != null)
            {
                foreach (GridAction Action in CustomActions)
                {
                    string URL = string.Format(Action.URL, ID);
                    string customAct = @"<a href=""{0}"" class=""btn btn-sm btn-icon btn-pure btn-default"" data-original-title=""{1}"">{1}</a>";
                    act += string.Format(customAct, URL, Action.Text);
                }
            }

            act += "</td></tr>";
            Body += string.Format(act, ControllerName, ID);
        }

        private void MakeField(string PropertyName, object Value, int ID)
        {
            // does it need Hyperlinks?
            if (PropertyName == Hyperlinks.FieldName && ID != 0)
            {
                string url = string.Format(Hyperlinks.URL, ID);
                Body += string.Format("<td><a href='{1}'>{0}</></td>", Value, url);
            }
            else
            {
                Body += string.Format("<td>{0}</td>", Value);
            }
        }

        private string PAGER = ""; // problem with 11
        private void MakePager()
        {
            // get the page query sting
            int ActivePage = 0;
            var qpage = HttpContext.Current.Request.QueryString["page"];
            if (qpage != null) ActivePage = int.Parse(qpage);
            if (ActivePage == 0) ActivePage = 1;
            //

            // if show pager is true
            if (ShowPager == true)
            {
                // find number of pages
                int NumberOfPage = (int)Math.Ceiling((double)Model.Count() / LimitNumberRows);

                // initialize the pager
                PAGER += "<center><div class='btn-group btn-group-sm' role='group'>";

                //if there is a limitation and number of pages is greater than limit
                if ((NumberOfPage > LimitNumberPages) && (LimitNumberPages > 0))
                {
                    // initial the list of pages
                    int[] numPageInHTMLs = new int[LimitNumberPages + 1];
                    // show the first two pages
                    numPageInHTMLs[0] = 1;
                    numPageInHTMLs[1] = 2;
                    // find the first elements of the array
                    if (((double)NumberOfPage / (double)LimitNumberPages) > ActivePage)
                    {
                        for (int i = 2; i < LimitNumberPages - 2; i++)
                        {
                            numPageInHTMLs[i] = i + 1;
                        }
                        numPageInHTMLs[LimitNumberPages - 2] = -1;
                    }
                    // find the last elements of the array
                    else if ((((double)NumberOfPage / (double)LimitNumberPages) < ActivePage) && (NumberOfPage - ActivePage < LimitNumberPages / 2))
                    {
                        numPageInHTMLs[2] = -1;
                        for (int i = 3; i <= LimitNumberPages - 2; i++)
                        {
                            numPageInHTMLs[i] = NumberOfPage - (LimitNumberPages - i);
                        }
                    }
                    // find the middle elements of the array
                    else
                    {
                        numPageInHTMLs[2] = -1;
                        numPageInHTMLs[3] = ActivePage - 2;
                        numPageInHTMLs[4] = ActivePage - 1;
                        numPageInHTMLs[5] = ActivePage;
                        numPageInHTMLs[6] = ActivePage + 1;
                        numPageInHTMLs[7] = ActivePage + 2;
                        numPageInHTMLs[8] = -1;
                    }
                    // show the last two pages
                    numPageInHTMLs[9] = NumberOfPage - 1;
                    numPageInHTMLs[10] = NumberOfPage;

                    // compile the pager
                    foreach (int pageInHTML in numPageInHTMLs)
                    {
                        AddPageToPager(ActivePage, pageInHTML);
                    }
                }
                else
                {
                    // if there is no limitation show all the pages
                    for (int i = 1; i <= NumberOfPage; i++)
                    {
                        AddPageToPager(ActivePage, i);
                    }
                }
                // close the pager tag
                PAGER += "</div></center><br/>";

                // add it to the grid
                HTML = HTML.Replace("{PAGER}", PAGER);
            }
            else
            {
                HTML = HTML.Replace("{PAGER}", "");
            }
            //

            // filter the model
            int skip = (this.LimitNumberRows * (ActivePage - 1));
            Model = Model.Skip(skip).Take(this.LimitNumberRows).ToList();
            //
        }

        private void AddPageToPager(int ActivePage, int pageInHTML)
        {
            if (pageInHTML > 0)
            {
                string cssclass = "";
                if (ActivePage == pageInHTML) cssclass = "active";

                if (PagerCustomLink == null)
                    PAGER += string.Format("<a type='button' href='?page={0}' class='btn btn-outline btn-default {1}'>{0}</a>", pageInHTML, cssclass);
                else
                    PAGER += string.Format("<a type='button' href='" + PagerCustomLink + "' class='btn btn-outline btn-default {1}'>{0}</a>", pageInHTML, cssclass);
            }
            else
            {
                PAGER += "<div type='button' class='btn btn-outline btn-default'>...</div>";
            }
        }

        private void UpdateFields()
        {
            // create an array with one more length
            string[] FieldsWithPrimaryKey = new string[Fields.Length + 1];
            // add the PrimaryKey to the new array
            FieldsWithPrimaryKey[0] = PrimaryKey;
            // copy all the fields after the PrimaryKey
            Array.Copy(Fields, 0, FieldsWithPrimaryKey, 1, Fields.Length);
            // clean the Fields array
            Fields = new string[Fields.Length + 1];
            // update the Fields array
            Fields = FieldsWithPrimaryKey;
        }

        private ForeignKeyParameter GetForeignKey(string TableName)
        {
            ForeignKeyParameter result = new ForeignKeyParameter();

            // check if there is any foreign key in FKPQuery
            if (ForeignKeys != null)
            {
                var FKPQuery = (from fk in ForeignKeys
                                where fk.TableName == TableName
                                select fk);

                if (FKPQuery.Any())
                {
                    result = FKPQuery.FirstOrDefault();
                }
            }

            return result;
        }
    }
}