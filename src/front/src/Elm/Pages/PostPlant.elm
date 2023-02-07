module Pages.PostPlant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), getAuthed, postAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, style)
import Http
import ImageList
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, mapCmd, subscriptionBase, updateBase)
import Main2 exposing (viewBase)
import NavBar exposing (plantsLink)
import PlantHelper exposing (PlantModel, plantDecoder, viewPlantBase)
import Utils exposing (SubmittedResult(..), flex, flex1, largeFont, smallMargin, submittedDecoder)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = NoPlant
    | Plant String PlantView


type alias PlantView =
    { plant : WebData (Maybe PlantModel), postResult : Maybe (WebData SubmittedResult) }



--update


type LocalMsg
    = NoOp
    | GotPlant (Result Http.Error (Maybe PlantModel))
    | Images ImageList.Msg
    | UpdatePrice Float
    | Submit
    | GotResult (Result Http.Error SubmittedResult)


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
                            ( authedPlant { plantView | plant = Error err }, Cmd.none )

                        Images imgEvent ->
                            case plantView.plant of
                                Loaded (Just pl) ->
                                    ( authedPlant <| { plantView | plant = Loaded <| Just { pl | images = ImageList.update imgEvent pl.images } }, Cmd.none )

                                _ ->
                                    noOp

                        UpdatePrice price ->
                            case plantView.plant of
                                Loaded (Just pl) ->
                                    ( authedPlant <| { plantView | plant = Loaded <| Just { pl | price = price }, postResult = Nothing }, Cmd.none )

                                _ ->
                                    noOp

                        Submit ->
                            case plantView.plant of
                                Loaded (Just pl) ->
                                    ( authedPlant <| { plantView | postResult = Just Loading }, submitCommand auth.token id pl.price )

                                _ ->
                                    noOp

                        GotResult (Ok res) ->
                            ( authedPlant <| { plantView | postResult = Just <| Loaded res }, Cmd.none )

                        GotResult (Err err) ->
                            ( authedPlant <| { plantView | postResult = Just <| Error err }, Cmd.none )

                        NoOp ->
                            noOp

                NoPlant ->
                    noOp

        _ ->
            noOp



--commands


getPlantCommand : String -> String -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder (Just 0) token)
    in
    getAuthed token (PreparedPlant plantId) expect Nothing |> mapCmd


submitCommand : String -> String -> Float -> Cmd Msg
submitCommand token plantId price =
    let
        expect =
            Http.expectJson GotResult submittedDecoder
    in
    postAuthed token (PostPlant plantId price) Http.emptyBody expect Nothing |> mapCmd



--view


view : Model -> Html Msg
view model =
    viewBase model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage _ page =
    let
        noplant =
            div [] [ text "Sorry, you cannot post this plant" ]
    in
    case page of
        NoPlant ->
            noplant

        Plant id plantWeb ->
            viewWebdata plantWeb.plant (viewPlant noplant id plantWeb.postResult)


viewPlant : Html LocalMsg -> String -> Maybe (WebData SubmittedResult) -> Maybe PlantModel -> Html Msg
viewPlant noplant id res plant =
    let
        plantUpdate str =
            case String.toFloat str of
                Just val ->
                    Main <| UpdatePrice val

                Nothing ->
                    Main NoOp
    in
    case plant of
        Just plantView ->
            viewPlantBase True plantUpdate (\msg -> Main <| Images msg) (viewButtons res id) plantView

        Nothing ->
            noplant |> Html.map Main


viewButtons : Maybe (WebData SubmittedResult) -> String -> Html Msg
viewButtons result id =
    let
        btns =
            div [ flex, style "margin" "3em", Flex.row, Flex.justifyEnd ]
                [ Button.linkButton
                    [ Button.primary
                    , Button.onClick <| Navigate ("/notPosted/" ++ id ++ "/edit")
                    , Button.attrs
                        [ smallMargin
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
                    , Button.onClick <| Main Submit
                    ]
                    [ text "Post" ]
                ]

        resultView =
            case result of
                Just res ->
                    case res of
                        Loading ->
                            [ viewWebdata res viewRes ]

                        _ ->
                            [ viewWebdata res viewRes, btns ]

                Nothing ->
                    [ btns ]
    in
    div [ flex, style "flex" "2", Flex.col ]
        resultView


viewRes : SubmittedResult -> Html msg
viewRes res =
    let
        baseView className message =
            div [ flex1 ] [ div [ largeFont, class className ] [ text message ] ]
    in
    case res of
        SubmittedSuccess msg ->
            baseView "bg-primary" msg

        SubmittedFail msg ->
            baseView "bg-warning" msg


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
            Plant plantId (PlantView Loading Nothing)


decodePlantId : D.Value -> Result D.Error String
decodePlantId flags =
    D.decodeValue (D.field "plantId" D.string) flags


subscriptions : Model -> Sub Msg
subscriptions model =
    subscriptionBase model Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
