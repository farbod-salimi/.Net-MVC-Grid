# .Net-MVC-Grid
.NET MVC Grid helps you to have a MVC/Bootstrap grid view.

# How to use
- import the grid class and the model in the view
```cs
@using Your.Models
@using Grid.Helpers
@model List<YourModel>
@ViewBag.Title = "Home"
```
- add the HTML
```html
    <div class="col-md-12">
        <!-- GRID -->
        <div id="grid">
            <!-- grid will come here -->
        </div>
        <!-- End GRID -->
    </div>
```
- add Razor code
```cs
@{
    Grid<YourModel> grid = new Grid<YourModel>();
    grid.Model = Model;
    grid.Actions = false;
    grid.ShortHeader = true;
    grid.ShowPrimaryKey = false;
    grid.ControllerName = "YourController";
    grid.PrimaryKey = "YourID";
    
    grid.ForeignKeys = new List<ForeignKeyParameter>
    {
        new ForeignKeyParameter {
        TableName = "Tenant",
        FieldName = "CompanyName",
        ModelType = typeof(Tenant)}
    };
    
    grid.Hyperlinks = new Hyperlink
    {
        FieldName = "YourName",
        URL = "/Your/Detail/{0}"
    };
    
    grid.Fields = new string[] {
        "YourName",
        "YourMobileID"
    };
    
    string g = grid.Compile();
    @Html.Raw(g)
}
```
- the final result would be :
```cs
@using Your.Models
@using Grid.Helpers
@model List<YourModel>
@ViewBag.Title = "Home"

    <div class="col-md-12">
        <!-- GRID -->
        <div id="grid">
            @{
                Grid<YourModel> grid = new Grid<YourModel>();
                grid.Model = Model;
                grid.Actions = false;
                grid.ShortHeader = true;
                grid.ShowPrimaryKey = false;
                grid.ControllerName = "YourController";
                grid.PrimaryKey = "YourID";
                
                grid.ForeignKeys = new List<ForeignKeyParameter>
                {
                    new ForeignKeyParameter {
                    TableName = "Tenant",
                    FieldName = "CompanyName",
                    ModelType = typeof(Tenant)}
                };
                
                grid.Hyperlinks = new Hyperlink
                {
                    FieldName = "YourName",
                    URL = "/Your/Detail/{0}"
                };
                
                grid.Fields = new string[] {
                    "YourFieldName",
                    "YourFieldPhone",
                    "YourFieldEmail"
                };
                
                @Html.Raw(grid.Compile())
            }
        </div>
        <!-- End GRID -->
    </div>
```

# Ajax example with paging
```cs
        public string MyGridAjaxProvider(int page)
        {
            List<Object> model = DataProvider.GetAllObjects().OrderByDescending(x => x.ObjectTimestamp).ToList();
            
            Grid<Object> grid = new Grid<Object>();
            grid.Model = model;
            grid.ShowActions = true;
            grid.ShowDefaultActions = false;
            grid.ShortHeader = true;
            grid.ControllerName = "ListOfObjects";
            grid.PrimaryKey = "ObjectId";
            grid.LimitNumberRows = 7;
            grid.LimitNumberPages = 10;
            grid.PagerCustomLink = "javascript:getAjaxGrid({0});";
            grid.Fields = new string[] {
                                "ObjectName",
                                "ObjectType",
                                "ObjectTimestamp"
                            };

            grid.CustomActions = new List<GridAction>
            {
                new GridAction
                {
                    Text = "Show Object",
                    URL = "javascript:getObject({0}, '')"
                }
           };

            return grid.Compile();
        }
```