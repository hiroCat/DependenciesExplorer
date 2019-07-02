open System.IO
open System.Xml
open XPlot.D3
open System

type paket = {
    version : string option 
    name : string
    dependencies : paket seq
    isTestPacket: bool 
}

type project = {
    name: string
    fullPath : string
}

type inputParams = {
    path : string option
    filterOutTestP : bool 
    filterOutExtP: bool  
}

module projectExtractor = 
    
    let getNames (p:string[]) = 
        p 
        |> Seq.map(fun x -> (x.Split('\\')
                             |> Seq.last),x)
    
    let fOther (inputS:seq<string*string>) = 
        inputS
        |> Seq.where(fun x -> let c =  x|> fst
                              c.StartsWith(".")
                              |> not)                  
    let dNames p = 
        Directory.GetDirectories(p) 
        |> getNames
        |> fOther

    let fNames p = 
        Directory.GetFiles(p)
        |> getNames
        |> fOther

    let getProjName (s:string) = 
        let np = s.Split('.') 
        let a = np.Length 
        np 
        |> Seq.take (a-1) 
        |> String.concat "."

    let extractCsproj (i:string*string)  = 
        let a = i |> fst
        a.Split('.') 
            |> Seq.last
            |> function 
                "csproj" ->  Some(i)
                | _ -> None

    let (|SeqEmpty|SeqNotEmpty|) l = 
        if Seq.isEmpty l then SeqEmpty
        else SeqNotEmpty
    
    let (|Csproj|_|) (i: seq<string*string>) = 
        let r = 
            i
            |> Seq.map extractCsproj
            |> Seq.choose id 
        match r with 
        | SeqEmpty ->  None
        | _ -> Some(r|> Seq.head)

    let rec getProjectsRec p = 
        let fN = fNames p 
        let dN = dNames p 
        match fN, dN with 
        | Csproj a , _ -> [Some({name=(a|> fst |> getProjName); fullPath=(a |>snd);})]
        | _, SeqNotEmpty -> dN 
                            |> Seq.collect (fun x -> getProjectsRec (snd x))
                            |> Seq.toList
        | _ -> [None]
    
    let getProjects p = getProjectsRec p |> Seq.choose id 

module paketConverter = 

    let isTestPackageN (p:string) = 
        let s = p.Split('.') |> Seq.last 
        match s with 
        | "Test" | "UnitTest"| "IntegrationTest" -> true
        | _ -> false

    let isTestPackageD (p:paket seq) = 
        p
        |> Seq.where(fun x -> x.name.Equals("xunit") || x.name.Equals("nunit"))
        |> Seq.isEmpty
        |> not

    let extractAttributes (atColl:XmlAttributeCollection) =
        let convAt = atColl
                     |> Seq.cast<XmlAttribute>
        let n = convAt |> Seq.tryFind(fun x -> x.Name.Equals("Include"))
        let v = convAt |> Seq.tryFind(fun x -> x.Name.Equals("Version"))
        match n,v with 
        | Some nNode , Some vNode -> Some({version=Some(vNode.Value); name=(nNode.Value); dependencies = Seq.empty; isTestPacket = false})
        | Some nNode , _  -> Some({version=None; name=nNode.Value; dependencies = Seq.empty; isTestPacket = false});
        | _ -> None

    let convertToPaket (project:project) = 
        let doc = new XmlDocument()
        doc.Load(project.fullPath);
        let nodeItemGroup = doc.SelectNodes("//ItemGroup/PackageReference")
        let nodeVersion = doc.SelectNodes("//PropertyGroup/Version")
        
        let d = nodeItemGroup 
                |> Seq.cast<XmlNode> 
                |> Seq.map(fun x -> extractAttributes x.Attributes)
                |> Seq.choose id

        let v = nodeVersion 
                |> Seq.cast<XmlNode> 
                |> Seq.tryHead
                |> function  
                    Some e -> Some(e.InnerText)
                    | None -> None
        {
            version=v; 
            name=project.name; 
            dependencies = d; 
            isTestPacket = isTestPackageN project.name || isTestPackageD d
        }

module createChart = 

    let l (xs: 'a list) = new System.Collections.Generic.List<'a>(xs)

    let createEdges (pList:paket seq) (n:string seq) (i:inputParams) = 
        pList
        |> Seq.collect(fun x -> if i.filterOutExtP then
                                    x.dependencies
                                    |> Seq.map(fun d -> if Seq.contains d.name n then 
                                                            Some(x.name,d.name)
                                                        else 
                                                            None)
                                    |> Seq.choose id
                                else 
                                    x.dependencies
                                    |> Seq.map(fun d -> Some(x.name,d.name))
                                    |> Seq.choose id
                        )

    let createNodes (pList:paket seq) (i:inputParams) = 
        if i.filterOutTestP then 
            pList |> Seq.where(fun x -> x.isTestPacket |> not)
        else 
            pList
        |> Seq.map(fun x -> x.name)

    let createChart (pList:paket seq)(inputArgs:inputParams) = 
        let n = createNodes pList inputArgs
        createEdges pList n inputArgs       
        |> Chart.Create
        |> Chart.WithHeight 1000
        |> Chart.WithWidth 1900
        |> Chart.WithGravity 0.0
        |> Chart.WithCharge 0.0
        |> Chart.WithEdgeOptions (fun e -> {defaultEdgeOptions with Distance = 400.0})
        |> Chart.WithNodeOptions(fun n -> {defaultNodeOptions with RadiusScale=1.0; Fill = {Red = 150uy; Green = 195uy; Blue=150uy};Label = Some({Text = n.Name; StyleAttrs = list.Empty})})
        |> Chart.Show

module consoleInput =
    let (|StringNotEmpty|_|) l = 
        if String.IsNullOrEmpty l || String.IsNullOrWhiteSpace l then None
        else Some l

    let transformToB s =
        match s with 
        | "y" -> true
        | _ -> false
    
    let getParams = 
        Console.WriteLine("------------------------------------------")
        Console.WriteLine("Hello welcome to the depencecy finder 2000")
        Console.WriteLine("------------------------------------------")
        Console.WriteLine("")
        Console.WriteLine("Please insert your source path:")
        let r = Console.ReadLine()
        Console.WriteLine("Do you want to filter out test projects? (y/n) (default y)")
        let t = Console.ReadLine()
        Console.WriteLine("Do you want to filter out external projects? (y/n) (default y) (not semantic validation, so it may lead to no dependencies on indiviual project analysis)")
        let e = Console.ReadLine()
        match r, t, e with 
        | StringNotEmpty _,StringNotEmpty _ ,StringNotEmpty _ -> {path = Some(r); filterOutTestP = (transformToB t); filterOutExtP = (transformToB e);}
        | StringNotEmpty _, _, _ ->  {path = Some(r); filterOutTestP = true; filterOutExtP = true;}
        | _ -> {path = None; filterOutTestP = true; filterOutExtP = true;}
        
module mainThing = 

    [<EntryPoint>]
    let main argv = 
        let p = consoleInput.getParams
        match p.path with 
        | None -> Console.WriteLine("Invalid path ")
        | Some p'-> let projectlist = projectExtractor.getProjects p'
                    let paketList = projectlist |> Seq.map paketConverter.convertToPaket
                    let line1 = sprintf "You have %i projects" (Seq.length projectlist)
                    let line2 = sprintf "%i are test projects" (paketList |> Seq.where(fun x -> x.isTestPacket) |> Seq.length)
                    System.Console.WriteLine(line1)
                    System.Console.WriteLine(line2)
                    createChart.createChart paketList p
        0
            
      