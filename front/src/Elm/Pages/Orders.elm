module Pages.Orders exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Checkbox as Checkbox
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), getAuthed, imagesDecoder)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, href, style)
import Http
import ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import NavBar exposing (ordersLink, viewNav)
import Pages.NotPosted exposing (bgTeal)
import Utils exposing (fillParent, flex, flex1, largeCentered, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



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
    , images : ImageList.Model
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
    | HideFullfilledChecked Bool
    | Images ImageList.Msg Int


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
            case msg of
                GotOrders (Ok res) ->
                    ( authed { model | data = Loaded res }, Cmd.none )

                GotOrders (Err res) ->
                    ( authed { model | data = Error }, Cmd.none )

                HideFullfilledChecked val ->
                    ( authed { model | hideFulfilled = val }, Cmd.none )

                Images imgEvent postId ->
                    case model.data of
                        Loaded orders ->
                            let
                                getPostId o =
                                    case o of
                                        Created c ->
                                            c.postId

                                        Delivering d ->
                                            d.postId

                                        Delivered d2 ->
                                            d2.postId

                                order =
                                    List.head (List.filter (\o -> getPostId o == postId) orders)

                                updateOrder o =
                                    if getPostId o == postId then
                                        case o of
                                            Created c ->
                                                Created { c | images = ImageList.update imgEvent c.images }

                                            Delivering d ->
                                                Delivering { d | images = ImageList.update imgEvent d.images }

                                            Delivered d2 ->
                                                Delivered { d2 | images = ImageList.update imgEvent d2.images }

                                    else
                                        o

                                updatedOrder =
                                    List.map updateOrder orders
                            in
                            ( authed { model | data = Loaded updatedOrder }, Cmd.none )

                        _ ->
                            noOp

                NoOp ->
                    noOp

        _ ->
            noOp



--commands


getOrders : String -> Bool -> Cmd Msg
getOrders token onlyMine =
    let
        expect =
            Http.expectJson GotOrders (ordersDecoder token)
    in
    getAuthed token (AllOrders onlyMine) expect Nothing


ordersDecoder : String -> D.Decoder (List Order)
ordersDecoder token =
    D.field "items" (D.list (orderDecoder token))


orderDecoder : String -> D.Decoder Order
orderDecoder token =
    D.field "status" D.int |> D.andThen (decoderSelector token)


decoderSelector : String -> Int -> D.Decoder Order
decoderSelector token status =
    let
        mapToCreated decoder =
            D.map Created decoder

        mapToDelivering decoder =
            D.map Delivering decoder

        mapToDelivered decoder =
            D.map Delivered decoder

        initedDecoder =
            orderDecoderBase token
    in
    case status of
        0 ->
            initedDecoder (D.succeed True) |> mapToCreated

        1 ->
            initedDecoder (deliveringDecoder (D.succeed True)) |> mapToDelivering

        2 ->
            initedDecoder (deliveringDecoder deliveredDecoder) |> mapToDelivered

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


orderDecoderBase : String -> D.Decoder a -> D.Decoder (OrderBase a)
orderDecoderBase token addDecoder =
    D.succeed OrderBase
        |> custom statusDecoder
        |> required "postId" D.int
        |> required "city" D.string
        |> required "mailNumber" D.int
        |> required "sellerName" D.string
        |> required "sellerContact" D.string
        |> required "price" D.float
        |> required "orderedDate" D.string
        |> custom (imagesDecoder token [ "images" ])
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
    let
        switchViewMessage =
            case page.viewType of
                ProducerView ->
                    "To Consumer View"

                ConsumerView ->
                    "To Producer View"

        switchViewHref =
            case page.viewType of
                ProducerView ->
                    "/orders"

                ConsumerView ->
                    "/orders/employee"

        checkAttrs =
            Checkbox.attrs [ bgTeal ]

        checksView =
            div [ flex, Flex.row, Flex.alignItemsCenter ]
                [ Checkbox.checkbox [ Checkbox.checked True, Checkbox.disabled True, checkAttrs ] ""
                , Checkbox.checkbox
                    [ Checkbox.checked page.hideFulfilled
                    , Checkbox.onCheck HideFullfilledChecked
                    , checkAttrs
                    ]
                    "Hide delivered"
                ]

        topViewItems =
            if page.showAdditional then
                [ checksView
                , div [] [ Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href switchViewHref ] ] [ text switchViewMessage ] ]
                ]

            else
                [ checksView ]

        topView =
            div [ flex, Flex.row, flex, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
                topViewItems
    in
    div ([ flex, Flex.col ] ++ fillParent)
        [ topView
        , viewWebdata page.data (mainView page.hideFulfilled)
        ]


mainView : Bool -> List Order -> Html Msg
mainView hide orders =
    let
        isNotDelivered order =
            case order of
                Delivered _ ->
                    False

                _ ->
                    True

        fOrders =
            if hide then
                List.filter isNotDelivered orders

            else
                orders
    in
    div [ flex, Flex.col, style "flex" "8", style "overflow-y" "scroll" ] (List.map viewOrder fOrders)


viewOrder : Order -> Html Msg
viewOrder order =
    case order of
        Created cr ->
            viewOrderBase False cr (\a -> []) (div [] [])

        Delivering del ->
            viewOrderBase False del (\a -> viewDelivering (\b -> div [] []) a) (div [] [])

        Delivered del ->
            viewOrderBase True del (\a -> viewDelivering viewDelivered a) (div [] [])


viewDelivered : DeliveredView -> Html Msg
viewDelivered del =
    viewInfoRow "Shipped" del.shipped


viewDelivering : (a -> Html Msg) -> DeliveringView a -> List (Html Msg)
viewDelivering add order =
    [ viewInfoRow "Delivery Started" order.deliveryStartedDate
    , viewInfoRow "Tracking Number" order.deliveryTrackingNumber
    , add order.additional
    ]


viewOrderBase : Bool -> OrderBase a -> (a -> List (Html Msg)) -> Html Msg -> Html Msg
viewOrderBase fill order viewAdd btnView =
    let
        imgCol =
            div [ flex, Flex.col, smallMargin, flex1 ]
                [ div largeCentered [ text ("#" ++ String.fromInt order.postId ++ " from " ++ order.orderedDate) ]
                , Html.map (\e -> Images e order.postId) (ImageList.view order.images)

                --TODO: Add image
                ]

        fillClass =
            if fill then
                bgTeal

            else
                class ""
    in
    div [ flex, Flex.row, flex1, fillClass, style "margin-bottom" "1.5rem", style "border-bottom" "solid 1px black" ]
        [ imgCol
        , infoCol order viewAdd btnView
        ]


infoCol : OrderBase a -> (a -> List (Html Msg)) -> Html Msg -> Html Msg
infoCol order viewAdd btnView =
    div [ flex, Flex.col, flex1 ]
        ([ viewInfoRow "Status" order.status
         , viewInfoRow "Delivery Address" (order.city ++ ", " ++ String.fromInt order.mailNumber)
         , viewInfoRow "Ordered From" order.sellerName
         , viewInfoRow "Vendor Contact" order.sellerContact
         , viewInfoRow "Cost" <| String.fromFloat order.price
         ]
            ++ viewAdd order.additional
            ++ [ div [ flex, Flex.row, Flex.alignItemsCenter, Flex.justifyCenter ]
                    [ btnView
                    ]
               ]
        )


viewInfoRow : String -> String -> Html msg
viewInfoRow desc val =
    div [ flex, Flex.row, flex, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
        [ div [ Utils.largeFont ] [ text desc ]
        , div [ Utils.largeFont, smallMargin ] [ text val ]
        ]



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
                        List.any (\a -> a == Producer || a == Manager) response.roles

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
