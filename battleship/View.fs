module View
open GameCore
open Model
open Microsoft.Xna.Framework

let (tilew,tileh) = 50,50
let (offsetx,offsety) = 50,50

let resolution = Windowed ((offsetx*2)+(tilew*boardx),(offsety*2)+(tileh*boardy))

let assetsToLoad = [
    Texture { key = "blank"; path = "Content/white" }
    Font { key = "default"; path = "Content/miramo" }
]

let getView runState model = 
    let currentPlayer = if model.isFirstPlayerTurn then fst model.players else snd model.players
    let shipTiles = 
        currentPlayer.ships |> List.collect (fun (name,tiles) ->
            let label = Seq.head name |> string
            tiles |> List.collect(fun t -> 
                let pos = offsetx+t.x*tilew+3+tilew/2,offsety+t.y*tileh+3+tileh/2
                let rect = offsetx+t.x*tilew+3,offsety+t.y*tileh+3,tilew-6,tileh-6
                [
                    ColouredImage (Color.Gray, { assetKey = "blank"; destRect = rect; sourceRect = None })
                    ColouredText (Color.White, { assetKey = "default"; text = label; position = pos; origin = Centre; scale = 0.2 })
                ]))

    [ColouredImage (Color.Black, { assetKey = "blank"; destRect = offsetx,offsety,tilew*boardx,tileh*boardy; sourceRect = None })]
    @ ([0..boardx-1] |> List.collect (fun x -> [0..boardy-1] |> List.map (fun y -> 
        let rect = offsetx+x*tilew+1, offsety+y*tileh+1,tilew-2,tileh-2
        ColouredImage (Color.Blue, { assetKey = "blank"; destRect = rect; sourceRect = None }))))
    @ shipTiles