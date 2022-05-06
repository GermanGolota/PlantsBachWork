module Pages.Orders exposing (..)

import Endpoints exposing (Endpoint(..), getAuthed)
import Html exposing (Html, div)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import NavBar exposing (ordersLink, viewNav)
import Webdata exposing (WebData(..))



--model


type alias Model =
    ModelBase View


type alias View =
    { data : WebData (List Order)
    , viewType : ViewType
    , showAdditional : Bool
    , hideFulfilled : Bool
    }


type ViewType
    = ConsumerView
    | ProducerView


type alias OrderBase a =
    { status : String
    , postId : Int
    , city : String
    , mailNumber : Int
    , sellerName : String
    , sellerContact : String
    , price : Float
    , orderedDate : String
    , additional : a
    }


type alias DeliveringView a =
    { deliveryStartedDate : String
    , deliveryTrackingNumber : String
    , additional : a
    }


type alias DeliveredView =
    { shipped : String
    }


type alias Delivered =
    OrderBase (DeliveringView DeliveredView)


type alias Delivering =
    OrderBase (DeliveringView Bool)


type alias Created =
    OrderBase Bool


type Order
    = Created Created
    | Delivering Delivering
    | Delivered Delivered



--update


type Msg
    = NoOp
    | GotOrders (Result Http.Error (List Order))


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    case m of
        Authorized auth model ->
            ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )



--commands


getOrders : String -> Bool -> Cmd Msg
getOrders token onlyMine =
    let
        expect =
            Http.expectJson GotOrders ordersDecoder
    in
    getAuthed token (AllOrders onlyMine) expect Nothing


ordersDecoder : D.Decoder (List Order)
ordersDecoder =
    D.field "items" (D.list orderDecoder)


orderDecoder : D.Decoder Order
orderDecoder =
    D.field "status" D.int |> D.andThen decoderSelector


decoderSelector : Int -> D.Decoder Order
decoderSelector status =
    let
        mapToCreated decoder =
            D.map Created decoder

        mapToDelivering decoder =
            D.map Delivering decoder

        mapToDelivered decoder =
            D.map Delivered decoder
    in
    case status of
        0 ->
            orderDecoderBase (D.succeed True) |> mapToCreated

        1 ->
            orderDecoderBase (deliveringDecoder (D.succeed True)) |> mapToDelivering

        2 ->
            orderDecoderBase (deliveringDecoder deliveredDecoder) |> mapToDelivered

        _ ->
            D.fail "unsupported status"


deliveredDecoder : D.Decoder DeliveredView
deliveredDecoder =
    D.map DeliveredView <| D.field "shippedDate" D.string


deliveringDecoder : D.Decoder a -> D.Decoder (DeliveringView a)
deliveringDecoder addDecoder =
    D.succeed DeliveringView
        |> required "deliveryStartedDate" D.string
        |> required "deliveryTrackingNumber" D.string
        |> custom addDecoder


orderDecoderBase : D.Decoder a -> D.Decoder (OrderBase a)
orderDecoderBase addDecoder =
    D.succeed OrderBase
        |> custom statusDecoder
        |> required "postId" D.int
        |> required "city" D.string
        |> required "mailNumber" D.int
        |> required "sellerName" D.string
        |> required "sellerContact" D.string
        |> required "price" D.float
        |> required "orderedDate" D.string
        |> custom addDecoder


statusDecoder : D.Decoder String
statusDecoder =
    let
        stToMessage st =
            case st of
                0 ->
                    "Created"

                1 ->
                    "Delivering"

                2 ->
                    "Delivered"

                _ ->
                    "Unknown"
    in
    D.map stToMessage <| D.field "status" D.int



--view


view : Model -> Html Msg
view model =
    viewNav model (Just ordersLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    div [] []



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        isProducer =
            case D.decodeValue (D.field "isEmployee" D.bool) flags of
                Ok res ->
                    res

                Err _ ->
                    False

        showAdditional =
            case resp of
                Just response ->
                    if isProducer then
                        List.member Consumer response.roles

                    else
                        List.member Producer response.roles

                _ ->
                    False

        viewType =
            if isProducer then
                ProducerView

            else
                ConsumerView

        onlyMine =
            case viewType of
                ProducerView ->
                    False

                ConsumerView ->
                    True

        initialCmd res =
            getOrders res.token onlyMine
    in
    initBase [ Producer, Consumer, Manager ] (View Loading viewType showAdditional False) initialCmd resp



--subs


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
