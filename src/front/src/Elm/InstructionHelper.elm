module InstructionHelper exposing (..)

import Endpoints exposing (Endpoint(..), getAuthed, instructioIdToCover)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, requiredAt)
import Utils exposing (existsDecoder)


type alias InstructionView =
    { id : Int
    , title : String
    , description : String
    , imageUrl : Maybe String
    , text : String
    , groupId : Int
    }


getInstruction : (Result Http.Error (Maybe InstructionView) -> msg) -> String -> Int -> Cmd msg
getInstruction cmd token id =
    let
        expect =
            Http.expectJson cmd (decodeInstruction token)
    in
    getAuthed token (GetInstruction id) expect Nothing


decodeInstruction : String -> D.Decoder (Maybe InstructionView)
decodeInstruction token =
    existsDecoder (decodeInstructionBase token)


decodeInstructionBase : String -> D.Decoder InstructionView
decodeInstructionBase token =
    let
        requiredItem name =
            requiredAt [ "item", name ]
    in
    D.succeed InstructionView
        |> requiredItem "id" D.int
        |> requiredItem "title" D.string
        |> requiredItem "description" D.string
        |> custom (coverDecoder token)
        |> requiredItem "instructionText" D.string
        |> requiredItem "plantGroupId" D.int


coverDecoder : String -> D.Decoder (Maybe String)
coverDecoder token =
    D.at [ "item", "hasCover" ] D.bool |> D.andThen (coverImageDecoder token)


coverImageDecoder token hasCover =
    if hasCover then
        D.map (\id -> Just (instructioIdToCover token id)) (D.at [ "item", "id" ] D.int)

    else
        D.succeed Nothing
