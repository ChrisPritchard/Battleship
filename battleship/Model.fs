module Model

let (boardx,boardy) = 10,10
let shipList = [5;4;4;3;3;2;2]

type GameModel = {
    players: PlayerState * PlayerState
    isFirstPlayerTurn: bool
} and PlayerState = {
    ships: Tile list list
    shots: Tile list
} and 
    Tile = { x: int; y: int }
type Dir = | North = 0 | East = 1 | South = 2 | West = 3

let nextIn direction tile = 
    match direction with
    | Dir.North -> { tile with y = tile.y - 1 }
    | Dir.East -> { tile with y = tile.x + 1 }
    | Dir.South -> { tile with y = tile.y + 1 }
    | _ -> { tile with y = tile.x - 1 }

let rec tilesIn direction length tile =
    let next = nextIn direction tile
    if next.x < 0 || next.x = boardx || next.y < 0 || next.y = boardy then
        []
    else if length = 1 then
        [next]
    else
        next::tilesIn direction (length - 1) next

let canPlace tile length direction board = 
    let tiles = tilesIn direction length tile
    List.length tiles = length && List.concat board |> List.exists (fun o -> List.contains o tiles) |> not

let randomPlacement () = 
    let random = new System.Random ()
    let rec findMatch length board =
        let tile = { 
            x = random.Next (0, boardx); 
            y = random.Next (0, boardy) } 
        let dir = enum<Dir>(random.Next(0,4))
        if canPlace tile length dir board 
        then tilesIn dir length tile
        else findMatch length board
    List.fold (fun board shipLength -> 
        board @ [findMatch shipLength board]) [] shipList