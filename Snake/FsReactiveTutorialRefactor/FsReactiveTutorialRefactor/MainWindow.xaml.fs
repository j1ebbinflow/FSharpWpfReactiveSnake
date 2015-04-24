namespace ViewModels

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open FSharp.ViewModule
open FSharp.ViewModule.Validation
open FsXaml

type MainView = XAML<"MainWindow.xaml", true>

type MainViewModel() as self = 
    inherit ViewModelBase()

type SnakeCanvas() as self = 
    inherit Canvas()
    let mutable visuals = new VisualCollection(self)
    
    do 
        self.Loaded.Add(fun _ -> self.Setup())
        ()

    member self.CreateRectangle() = 
        let visual = new DrawingVisual()
        use dc = visual.RenderOpen()
        let rect = new Rect(new Point(100.0,100.0), new Size(100.0,100.0))
        dc.DrawRectangle(Brushes.Black,new Pen(Brushes.Black,2.0),rect)
        dc.Close()
        visual

    member this.Setup() = 
        visuals.Add(self.CreateRectangle()) |> ignore
        
    
