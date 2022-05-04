module Pages.PostPlant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), getAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (href, style)
import Http
import ImageList
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import NavBar exposing (plantsLink, viewNav)
import Pages.Plant exposing (PlantModel, plantDecoder, viewPlantBase)
import Utils exposing (SubmittedResult, flex, largeFont, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = NoPlant
    | Plant Int PlantView


type alias PlantView =
    { plant : WebData (Maybe PlantModel), postResult : Maybe (WebData SubmittedResult) }



--update


type Msg
    = NoOp
    | GotPlant (Result Http.Error (Maybe PlantModel))
    | Images ImageList.Msg
    | UpdatePrice Float


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
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
            case model of
                Plant id plantView ->
                    let
                        authedPlant pl =
                            authed <| Plant id pl
                    in
                    case msg of
                        GotPlant (Ok res) ->
                            ( authedPlant { plantView | plant = Loaded res }, Cmd.none )

                        GotPlant (Err err) ->
                            ( authedPlant { plantView | plant = Error }, Cmd.none )

                        Images imgEvent ->
                            case plantView.plant of
                                Loaded (Just pl) ->
                                    ( authedPlant <| { plantView | plant = Loaded <| Just { pl | images = ImageList.update imgEvent pl.images } }, Cmd.none )

                                _ ->
                                    noOp

                        UpdatePrice price ->
                            case plantView.plant of
                                Loaded (Just pl) ->
                                    ( authedPlant <| { plantView | plant = Loaded <| Just { pl | price = price } }, Cmd.none )

                                _ ->
                                    noOp

                        NoOp ->
                            noOp

                NoPlant ->
                    noOp

        _ ->
            noOp



--commands


getPlantCommand : String -> Int -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder (Just 0) token)
    in
    getAuthed token (PreparedPlant plantId) expect Nothing



--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    let
        noplant =
            div [] [ text "Sorry, you cannot post this plant" ]
    in
    case page of
        NoPlant ->
            noplant

        Plant id plantWeb ->
            viewWebdata plantWeb.plant (viewPlant noplant id)


viewPlant : Html Msg -> Int -> Maybe PlantModel -> Html Msg
viewPlant noplant id plant =
    let
        plantUpdate str =
            case String.toFloat str of
                Just val ->
                    UpdatePrice val

                Nothing ->
                    NoOp
    in
    case plant of
        Just plantView ->
            viewPlantBase True plantUpdate Images (viewButtons id) plantView

        Nothing ->
            noplant


viewButtons : Int -> Html Msg
viewButtons id =
    let
        postOnClick =
            Button.onClick NoOp
    in
    div [ flex, style "margin" "3em", Flex.row, Flex.justifyEnd ]
        [ Button.linkButton
            [ Button.primary
            , Button.attrs
                [ smallMargin
                , href ("/notPosted/" ++ String.fromInt id ++ "/edit")
                , largeFont
                ]
            ]
            [ text "Edit" ]
        , Button.button
            [ Button.primary
            , Button.attrs
                [ smallMargin
                , largeFont
                ]
            , postOnClick
            ]
            [ text "Post" ]
        ]


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initial =
            decodeInitial flags

        initialCmd res =
            case initial of
                Plant id _ ->
                    getPlantCommand res.token id

                NoPlant ->
                    Cmd.none
    in
    initBase [ Producer, Consumer, Manager ] initial initialCmd resp


decodeInitial : D.Value -> View
decodeInitial flags =
    case decodePlantId flags of
        Err _ ->
            NoPlant

        Ok plantId ->
            case String.toInt plantId of
                Just id ->
                    Plant id (PlantView Loading Nothing)

                Nothing ->
                    NoPlant


decodePlantId : D.Value -> Result D.Error String
decodePlantId flags =
    D.decodeValue (D.field "plantId" D.string) flags


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
