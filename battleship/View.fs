module View
open GameCore
open Model
open Microsoft.Xna.Framework

let resolution = Windowed (1200,600)
let playerShotsOffset = 500,50
let playerShotsTileSize = 50,50

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

let renderPlacement runState model (dir,rem) = 
    let (ox,oy) = 350,50
    let tileSize = 50,50
    
    let playerShipTiles = model.player.ships |> List.collect (shipTiles (ox,oy) tileSize)
    
    let (rx,ry) = ox + 470,oy
    let spacing = 44

    let tileSizeSm = 40,40
    let remainingShips = 
        rem |> List.indexed |> List.collect (fun (i,len) ->
            let (x,y) = rx,ry + (i*spacing)
            let tiles = tilesIn { x = 0; y = 0} Dir.East len
            shipTiles (x,y) tileSizeSm ((nameForLength len),tiles))
    
    boardTiles (ox,oy) tileSize @ playerShipTiles @ remainingShips

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