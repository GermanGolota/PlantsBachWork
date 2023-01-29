module Pages.NotPosted exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Form.Checkbox as Checkbox
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), getAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, style)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (required)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, mapCmd, updateBase)
import NavBar exposing (plantsLink, viewNav)
import Utils exposing (bgTeal, chunkedView, decodeId, fillParent, flex, flex1, largeFont, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type alias View =
    { items : WebData (List PlantItem), onlyMine : Bool }


type alias PlantItem =
    { id : String
    , name : String
    , description : String
    , isMine : Bool
    }



--update


type LocalMsg
    = GotPlants (Result Http.Error (List PlantItem))
    | OnlyMineChecked Bool


type alias Msg =
    MsgBase LocalMsg


update : Msg -> Model -> ( Model, Cmd Msg )
update =
    updateBase updateLocal


updateLocal : LocalMsg -> Model -> ( Model, Cmd Msg )
updateLocal msg m =
    let
        noOp =
            ( m, Cmd.none )
    in
    case m of
        Authorized auth model ->
            let
                authed =
                    Authorized auth
            in
            case msg of
                GotPlants (Ok res) ->
                    ( authed <| { model | items = Loaded res }, Cmd.none )

                GotPlants (Err res) ->
                    ( authed <| { model | items = Error }, Cmd.none )

                OnlyMineChecked val ->
                    ( authed <| { model | onlyMine = val }, Cmd.none )

        _ ->
            noOp



--commands
--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    let
        viewChunked items =
            chunkedView 3 viewItem (List.filter (filterMine page.onlyMine) items)
    in
    div (fillParent ++ [ flex, Flex.col ])
        [ div [ flex, style "flex" "13", Flex.justifyBetween, Flex.row ]
            [ Button.linkButton
                [ Button.primary
                , Button.onClick <| Navigate "/notPosted/add"
                , Button.attrs
                    [ smallMargin
                    , largeFont
                    , bgTeal
                    ]
                ]
                [ text "Add Plant" ]
            , div [ flex, Flex.row, Flex.alignItemsCenter ]
                [ div [ smallMargin ] [ text "Only Mine" ]
                , Checkbox.checkbox [ Checkbox.attrs [ bgTeal ], Checkbox.onCheck (\val -> Main <| OnlyMineChecked val), Checkbox.checked page.onlyMine ] ""
                , Checkbox.checkbox [ Checkbox.checked True, Checkbox.disabled True, Checkbox.attrs [ bgTeal ] ] ""
                ]
            ]
        , div [ flex, style "flex" "87", Flex.row ]
            [ viewWebdata page.items viewChunked
            ]
        ]


filterMine onlyMine item =
    (onlyMine && item.isMine) || (onlyMine == False)


viewItem item =
    let
        bgFill =
            if item.isMine then
                bgTeal

            else
                class ""
    in
    Card.config [ Card.attrs (fillParent ++ [ flex1, bgFill ]) ]
        |> Card.header [ class "text-center" ]
            [ text item.name
            ]
        |> Card.block []
            [ Block.text [] [ text item.description ]
            , Block.custom <|
                div [ flex, Flex.row, Flex.justifyEnd, Flex.alignItemsCenter ]
                    [ Button.linkButton [ Button.primary, Button.onClick <| Navigate ("/notPosted/" ++ item.id ++ "/edit"), Button.attrs [ smallMargin ] ] [ text "Edit" ]
                    , Button.linkButton [ Button.primary, Button.onClick <| Navigate ("/notPosted/" ++ item.id ++ "/post"), Button.attrs [ smallMargin ] ] [ text "Post" ]
                    ]
            ]
        |> Card.view



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    initBase [ Producer, Consumer, Manager ] (View Loading False) (\res -> plantsCmd res.token) resp



--cmds


plantsCmd : String -> Cmd Msg
plantsCmd token =
    let
        expect =
            Http.expectJson GotPlants plantsDecoder
    in
    getAuthed token NotPostedPlants expect Nothing |> mapCmd


plantsDecoder =
    D.field "items" (D.list plantDecoder)


plantDecoder =
    D.succeed PlantItem
        |> required "id" decodeId
        |> required "plantName" D.string
        |> required "description" D.string
        |> required "isMine" D.bool


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
