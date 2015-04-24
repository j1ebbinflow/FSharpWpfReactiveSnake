module main

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes
open FsXaml
open ViewModels

type App = XAML<"App.xaml">
type MainView = XAML<"MainWindow.xaml", true>

type DragActive = |CurrentlyDragging |NotDragging
type DragState = { 
    dragging: DragActive; 
    position: Point; 
    offset: Point 
}

type DragChange = 
    | StartDrag of Point
    | UpdatePosition of Point
    | StopDrag

let initialState = { dragging=NotDragging; position=new Point(); offset=new Point() }

let currentlyDragging (state: DragState) = 
    match state.dragging with
    | CurrentlyDragging -> true
    | NotDragging -> false

let getDragPosition (state: DragState) = 
    let diff = state.position - state.offset
    new Point(diff.X, diff.Y)

let updateDragState state change = 
    match change with
    | StartDrag(offset) -> { state with dragging=CurrentlyDragging; offset=offset }
    | StopDrag -> {state with dragging=NotDragging}
    | UpdatePosition(pos) -> 
        match state.dragging with
        | CurrentlyDragging -> {state with position=pos}
        | NotDragging -> state

let setNewRectanglePosition rect (position : Point) = 
    Canvas.SetLeft(rect, position.X)
    Canvas.SetTop(rect, position.Y)

let createNewRectangleAt (canvas : Canvas) (position : Point)= 
    let rect = new System.Windows.Shapes.Rectangle()
    rect.Fill <- Brushes.Black
    rect.Width <- 20.0
    rect.Height <- 20.0
    rect.RadiusX <- 10.0
    rect.RadiusY <- 10.0
    setNewRectanglePosition rect position
    canvas.Children.Add(rect) |> ignore

let initializeWindow() =
    let view = MainView()

    let getMousePositionRelativeTo element (args : Input.MouseEventArgs) =
        args.GetPosition element
    
    let getMousePositionRelativeToRectangle = getMousePositionRelativeTo view.Rectangle
    let getMousePositionRelativeToCanvas = getMousePositionRelativeTo view.Canvas
    let setRectanglePosition (position : Point) = 
        Canvas.SetLeft(view.Rectangle, position.X)
        Canvas.SetTop(view.Rectangle, position.Y)

    let startDragEventStream = 
        view.Rectangle.MouseLeftButtonDown
        |> Observable.map (getMousePositionRelativeToRectangle >> StartDrag)

    let stopDragEventStream = 
        view.Canvas.MouseLeftButtonUp
        |> Observable.map (fun _ -> StopDrag)

    let updateDragPositionEventStream = 
        view.Canvas.MouseMove
        |> Observable.map(getMousePositionRelativeToCanvas >> UpdatePosition)

    let dragEventSubscription = 
        Observable.merge startDragEventStream stopDragEventStream |> Observable.merge updateDragPositionEventStream
        |> Observable.scan updateDragState initialState
        |> Observable.filter currentlyDragging
        |> Observable.map getDragPosition
        |> Observable.subscribe setRectanglePosition

    let createNewRectanglesOnCanvas = createNewRectangleAt view.Canvas

    let rightClickEventStream = 
        view.Canvas.MouseRightButtonDown
        |> Observable.map (getMousePositionRelativeToCanvas)
        |> Observable.subscribe(createNewRectanglesOnCanvas)

    let arrowKeyPressEventStream = 
        view.Canvas.KeyDown 
        |> Observable.filter (isArrowKeyPress)

    view.Root

[<STAThread>]
[<EntryPoint>]
(new Application()).Run(initializeWindow()) |> ignore