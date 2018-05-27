module View
open GameCore
open Model
open Microsoft.Xna.Framework

let resolution = Windowed (1200,600)
let placementOffset = 350,50
let placementTilesize = 50,50
let playerShotsOffset = 500,50
let playerShotsTileSize = 50,50

let findMouseTile (runState: RunState) (offsetx,offsety) (tilew,tileh) =
    let (mx,my) = runState.mouse.position
    let (rawx,rawy) = mx - offsetx, my - offsety
    let (tx,ty) = floor (float rawx / float tilew) |> int, floor (float rawy / float tileh) |> int
    if tx < 0 || tx >= boardx || ty < 0 || ty >= boardy then None else Some { x = tx; y = ty }

let assetsToLoad = [
    Texture { key = "blank"; path = "Content/white" }
    Texture { key = "explosion"; path = "Content/explosion" }
    Texture { key = "cursor"; path = "Content/cursor" }
    Font { key = "default"; path = "Content/miramo" }
]

let boardTiles (offsetx,offsety) (tilew,tileh) = 
    let boardSize = offsetx, offsety, tilew*boardx, tileh*boardy
    let back = ColouredImage (Color.Black, { assetKey = "blank"; destRect = boardSize; sourceRect = None })
    let sea = 
        [0..boardx-1] |> List.collect (fun x -> [0..boardy-1] |> List.map (fun y -> 
            let rect = offsetx+x*tilew+1, offsety+y*tileh+1, tilew-2, tileh-2
            ColouredImage (Color.Blue, { assetKey = "blank"; destRect = rect; sourceRect = None })))
    back::sea

let shipTiles (offsetx,offsety) (tilew,tileh) (shipName,tiles) = 
    let label = if shipName = "" then "" else Seq.head shipName |> string
    tiles |> List.collect(fun t -> 
        let pos = offsetx+t.x*tilew+3+tilew/2,offsety+t.y*tileh+3+tileh/2
        let rect = offsetx+t.x*tilew+3,offsety+t.y*tileh+3,tilew-6,tileh-6
        [
            ColouredImage (Color.Gray, { assetKey = "blank"; destRect = rect; sourceRect = None })
            ColouredText (Color.White, { assetKey = "default"; text = label; position = pos; origin = Centre; scale = 0.2 })
        ])

let shotTiles (offsetx,offsety) (tilew,tileh) shots targets =
    shots |> List.map (fun t ->
        let rect = offsetx+t.x*tilew,offsety+t.y*tileh,tilew,tileh
        if List.contains t targets then
            ColouredImage (Color.Yellow, { assetKey = "explosion"; destRect = rect; sourceRect = None })
        else
            Image { assetKey = "explosion"; destRect = rect; sourceRect = None })

let playerShips model =
    let offset = 50,50
    let tileSize = 40,40
    
    let playerShipTiles = model.player.ships |> List.collect (shipTiles offset tileSize)
    let shipTiles = model.player.ships |> List.collect (fun (_,t) -> t)
    let aiShots = shotTiles offset tileSize model.ai.shots shipTiles
    boardTiles offset tileSize @ playerShipTiles @ aiShots

let playerShots model = 
    let offset = playerShotsOffset
    let tileSize = playerShotsTileSize

    let aiHitShipTiles = model.ai.ships |> List.collect (fun (shipName,tiles) ->
        let hit = tiles |> List.filter (fun t -> List.contains t model.player.shots)
        let name = if List.length hit = List.length tiles then shipName else ""
        shipTiles offset tileSize (name,hit))
    let aiShipTiles = model.ai.ships |> List.collect (fun (_,t) -> t)
    let shots = shotTiles offset tileSize model.player.shots aiShipTiles
    boardTiles offset tileSize @ aiHitShipTiles @ shots

let renderTitle () =
    []

let renderTargetHighlight dir len runState offset tileSize board = 
    match findMouseTile runState offset tileSize with
    | None -> []
    | Some t ->
        let tiles = tilesIn t dir len
        shipTiles offset tileSize ((nameForLength len), tiles)
        |> List.map (fun img -> 
            match img with
            | ColouredImage (_,tx) ->
                let colour = 
                    if canPlace tiles board then 
                        Color.Green else Color.Red
                ColouredImage (colour,tx)
            | _ -> img)

let renderPlacement runState model (dir,rem) = 
    let (ox,oy) = placementOffset
    let tileSize = placementTilesize
    let head = List.tryHead rem
    
    let playerShipTiles = model.player.ships |> List.collect (shipTiles (ox,oy) tileSize)
    let target = 
        match head with
        | None -> []
        | Some len -> renderTargetHighlight dir len runState (ox,oy) tileSize model.player.ships

    let (rx,ry) = ox + 505,oy
    let spacing = 44

    let (tx,ty) = 40,40
    let remainingShips = 
        rem |> List.indexed |> List.collect (fun (i,len) ->
            let (x,y) = rx,ry + (i*spacing)
            let tiles = tilesIn { x = 0; y = 0} Dir.East len
            shipTiles (x,y) (tx,ty) ((nameForLength len),tiles))
    let highLight = 
        match head with
        | None -> []
        | Some len ->
            let (hx,hy,hw,hh) = rx-2,ry-2,(tx * len)+4,ty+4
            [ ColouredImage (Color.Red, { assetKey = "blank"; destRect = (hx,hy,hw,hh); sourceRect = None });
            ColouredImage (Color.White, { assetKey = "blank"; destRect = (hx+2,hy+2,hw-4,hh-4); sourceRect = None }) ]
    
    boardTiles (ox,oy) tileSize @ playerShipTiles @ target
    @ highLight @ remainingShips

let renderPlayerTurn model = 
    playerShips model @ playerShots model

let renderAiTurn model = 
    playerShips model @ playerShots model

let renderGameOver model = 
    []

let getView runState model = 
    let (mx,my) = runState.mouse.position
    let cursor = Image { assetKey = "cursor"; destRect = mx-16,my-16,32,32; sourceRect = None }

    let screen =
        match model.state with
        | Title -> renderTitle ()
        | Placement (dir,rem) -> renderPlacement runState model (dir,rem)
        | PlayerTurn -> renderPlayerTurn model
        | AITurn _ -> renderAiTurn model
        | GameOver -> renderGameOver model

    screen @ [cursor]