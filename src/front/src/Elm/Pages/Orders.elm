module Pages.Orders exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Checkbox as Checkbox
import Bootstrap.Form.Input as Input
import Bootstrap.Utilities.Flex as Flex
import Dict exposing (Dict)
import Endpoints exposing (Endpoint(..), getAuthed, historyUrl, imagesDecoder, postAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, href, style)
import Http
import ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, isAdmin, mapCmd, updateBase)
import NavBar exposing (ordersLink, viewNav)
import Utils exposing (bgTeal, decodeId, fillParent, flex, flex1, formatPrice, mediumCentered, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type alias View =
    { data : WebData (List Order)
    , viewType : ViewType
    , showAdditional : Bool
    , hideFulfilled : Bool
    , selectedTtns : Dict String String
    , rejected : Dict String (WebData Bool)
    , confirmed : Dict String (WebData Bool)
    }


type ViewType
    = ConsumerView
    | ProducerView


type alias OrderBase a =
    { status : String
    , postId : String
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


type LocalMsg
    = NoOp
    | GotOrders (Result Http.Error (List Order))
    | HideFullfilledChecked Bool
    | Images ImageList.Msg String
    | SelectedTtn String String
    | ConfirmSend String
    | GotConfirmSend String (Result Http.Error Bool)
    | ConfirmReceived String
    | GotConfirmReceived String (Result Http.Error Bool)
    | Reject String
    | GotReject String (Result Http.Error Bool)


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

                SelectedTtn postId value ->
                    let
                        updatedView =
                            Dict.union (Dict.fromList [ ( postId, value ) ]) model.selectedTtns
                    in
                    ( authed { model | selectedTtns = updatedView }, Cmd.none )

                ConfirmSend orderId ->
                    let
                        ttn =
                            Maybe.withDefault "" <| Dict.get orderId model.selectedTtns

                        updatedView =
                            Dict.union (Dict.fromList [ ( orderId, Loading ) ]) model.confirmed
                    in
                    ( authed { model | confirmed = updatedView }, startDelivery auth.token orderId ttn )

                GotConfirmSend orderId (Ok res) ->
                    let
                        updatedView =
                            Dict.union (Dict.fromList [ ( orderId, Loaded res ) ]) model.confirmed
                    in
                    ( authed { model | confirmed = updatedView }, getData auth.token model.viewType )

                GotConfirmSend orderId (Err err) ->
                    let
                        updatedView =
                            Dict.union (Dict.fromList [ ( orderId, Error ) ]) model.confirmed
                    in
                    ( authed { model | confirmed = updatedView }, Cmd.none )

                ConfirmReceived orderId ->
                    let
                        updatedView =
                            Dict.union (Dict.fromList [ ( orderId, Loading ) ]) model.confirmed
                    in
                    ( authed { model | confirmed = updatedView }, confirmDelivery auth.token orderId )

                GotConfirmReceived orderId (Ok res) ->
                    let
                        updatedView =
                            Dict.union (Dict.fromList [ ( orderId, Loaded res ) ]) model.confirmed
                    in
                    ( authed { model | confirmed = updatedView }, getData auth.token model.viewType )

                GotConfirmReceived orderId (Err err) ->
                    let
                        updatedView =
                            Dict.union (Dict.fromList [ ( orderId, Error ) ]) model.confirmed
                    in
                    ( authed { model | confirmed = updatedView }, Cmd.none )

                Reject orderId ->
                    ( authed <| { model | rejected = Dict.insert orderId Loading model.rejected }, rejectOrder auth.token orderId )

                GotReject orderId (Ok res) ->
                    ( authed <| { model | rejected = Dict.insert orderId (Loaded res) model.rejected }, getData auth.token model.viewType )

                GotReject orderId (Err err) ->
                    ( authed <| { model | rejected = Dict.insert orderId Error model.rejected }, Cmd.none )

                NoOp ->
                    noOp

        _ ->
            noOp


getPostId : Order -> String
getPostId o =
    case o of
        Created c ->
            c.postId

        Delivering d ->
            d.postId

        Delivered d2 ->
            d2.postId



--commands


rejectOrder : String -> String -> Cmd Msg
rejectOrder token orderId =
    postAuthed token (RejectOrder orderId) Http.emptyBody (Http.expectJson (GotReject orderId) (D.field "success" D.bool)) Nothing |> mapCmd


confirmDelivery : String -> String -> Cmd Msg
confirmDelivery token orderId =
    let
        expect =
            Http.expectJson (GotConfirmReceived orderId) (D.field "successfull" D.bool)
    in
    postAuthed token (ReceivedOrder orderId) Http.emptyBody expect Nothing |> mapCmd


startDelivery : String -> String -> String -> Cmd Msg
startDelivery token orderId ttn =
    let
        expect =
            Http.expectJson (GotConfirmSend orderId) (D.field "successfull" D.bool)
    in
    postAuthed token (SendOrder orderId ttn) Http.emptyBody expect Nothing |> mapCmd


getOrders : String -> Bool -> Cmd Msg
getOrders token onlyMine =
    let
        expect =
            Http.expectJson GotOrders (ordersDecoder token)
    in
    getAuthed token (AllOrders onlyMine) expect Nothing |> mapCmd


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
        |> required "postId" decodeId
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

        viewLocation =
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
                    , Checkbox.onCheck (\check -> Main <| HideFullfilledChecked check)
                    , checkAttrs
                    ]
                    "Hide delivered"
                ]

        topViewItems =
            if page.showAdditional then
                [ checksView
                , div [] [ Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href viewLocation ] ] [ text switchViewMessage ] ]
                ]

            else
                [ checksView ]

        topView =
            div [ flex, Flex.row, flex, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
                topViewItems
    in
    div ([ flex, Flex.col ] ++ fillParent)
        [ topView
        , viewWebdata page.data (mainView (isAdmin resp) page.confirmed page.rejected page.selectedTtns page.viewType page.hideFulfilled)
        ]


mainView : Bool -> Dict String (WebData Bool) -> Dict String (WebData Bool) -> Dict String String -> ViewType -> Bool -> List Order -> Html Msg
mainView isAdmin confirmed rejected ttns viewType hide orders =
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
    div [ flex, Flex.col, style "flex" "8", style "overflow-y" "scroll" ] (List.map (viewOrder isAdmin confirmed rejected ttns viewType) fOrders)


viewOrder : Bool -> Dict String (WebData Bool) -> Dict String (WebData Bool) -> Dict String String -> ViewType -> Order -> Html Msg
viewOrder isAdmin confirmed rejected ttns viewType order =
    let
        ttn =
            Maybe.withDefault "" <| Dict.get (getPostId order) ttns

        orderId =
            getPostId order

        confirmText isSuccess =
            if isSuccess then
                "Successfully confirmed!"

            else
                "Failed to confirm!"

        historyBtn =
            if isAdmin then
                Button.linkButton
                    [ Button.outlinePrimary
                    , Button.onClick <| Navigate <| historyUrl "PlantOrder" orderId
                    , Button.attrs [ smallMargin ]
                    ]
                    [ text "View history" ]

            else
                div [] []
    in
    case order of
        Created cr ->
            let
                rejectText isSuccess =
                    if isSuccess then
                        "Successfully rejected!"

                    else
                        "Failed to reject!"

                rejectRes =
                    case Dict.get orderId rejected of
                        Just val ->
                            viewWebdata val (\succ -> div mediumCentered [ text <| rejectText succ ])

                        Nothing ->
                            div [] []

                confirmRes =
                    case Dict.get orderId confirmed of
                        Just val ->
                            viewWebdata val (\succ -> div mediumCentered [ text <| rejectText succ ])

                        Nothing ->
                            div [] []

                isNotLoading dict =
                    case Dict.get orderId dict of
                        Just data ->
                            case data of
                                Loading ->
                                    False

                                _ ->
                                    True

                        Nothing ->
                            True

                shouldShowButtons =
                    isNotLoading rejected && isNotLoading confirmed

                producerViewBtns =
                    if shouldShowButtons then
                        [ Button.button [ Button.danger, Button.onClick <| Main <| Reject orderId, Button.attrs fillParent ] [ text "Reject" ]
                        , div (mediumCentered ++ [ smallMargin ]) [ text "Tracking Number" ]
                        , Input.text [ Input.onInput <| SelectedTtn (getPostId order), Input.value ttn ] |> Html.map Main
                        , Button.button
                            [ Button.primary, Button.onClick <| Main <| ConfirmSend (getPostId order), Button.attrs [ smallMargin ] ]
                            [ text "Confirm Send" ]
                        ]

                    else
                        []

                btns =
                    case viewType of
                        ProducerView ->
                            div [ flex, Flex.col, Flex.alignItemsCenter ]
                                [ div [ flex, Flex.row, flex1 ] ([ rejectRes, confirmRes ] ++ producerViewBtns)
                                , historyBtn
                                ]

                        ConsumerView ->
                            div []
                                [ historyBtn
                                ]
            in
            viewOrderBase False cr (\a -> []) btns

        Delivering del ->
            let
                resultView result =
                    viewWebdata result (\succ -> div mediumCentered [ text <| confirmText succ ])

                baseConsumerBtns =
                    div [ flex, Flex.col, flex1, smallMargin ]
                        [ Button.button
                            [ Button.onClick (ConfirmReceived <| getPostId order)
                            , Button.primary
                            ]
                            [ text "Confirm Received" ]
                            |> Html.map Main
                        , historyBtn
                        ]

                consumerBtns =
                    case Dict.get orderId confirmed of
                        Just data ->
                            case data of
                                Loading ->
                                    [ resultView data ]

                                _ ->
                                    [ resultView data, baseConsumerBtns ]

                        Nothing ->
                            [ baseConsumerBtns ]

                btns =
                    case viewType of
                        ConsumerView ->
                            div [ flex, Flex.col, flex1, smallMargin ] consumerBtns

                        ProducerView ->
                            div []
                                [ historyBtn
                                ]
            in
            viewOrderBase False del (\a -> viewDelivering (\b -> div [] []) a) btns

        Delivered del ->
            viewOrderBase True del (\a -> viewDelivering viewDelivered a) (div [] [ historyBtn ])


viewDelivered : DeliveredView -> Html LocalMsg
viewDelivered del =
    viewInfoRow "Shipped" del.shipped


viewDelivering : (a -> Html LocalMsg) -> DeliveringView a -> List (Html LocalMsg)
viewDelivering add order =
    [ viewInfoRow "Delivery Started" order.deliveryStartedDate
    , viewInfoRow "Tracking Number" order.deliveryTrackingNumber
    , add order.additional
    ]


viewOrderBase : Bool -> OrderBase a -> (a -> List (Html LocalMsg)) -> Html Msg -> Html Msg
viewOrderBase fill order viewAdd btnView =
    let
        imgCol =
            div [ flex, Flex.col, smallMargin, flex1 ]
                [ div mediumCentered [ text ("#" ++ order.postId ++ " from " ++ order.orderedDate) ]
                , Html.map (\e -> Images e order.postId) (ImageList.view order.images)
                ]

        fillClass =
            if fill then
                bgTeal

            else
                class ""
    in
    div [ flex, Flex.row, flex1, fillClass, style "margin-bottom" "1.5rem", style "border-bottom" "solid 1px black" ]
        [ imgCol |> Html.map Main
        , infoCol order viewAdd btnView
        ]


infoCol : OrderBase a -> (a -> List (Html LocalMsg)) -> Html Msg -> Html Msg
infoCol order viewAdd btnView =
    div [ flex, Flex.col, flex1 ]
        ([ viewInfoRow "Status" order.status
         , viewInfoRow "Delivery Address" (order.city ++ ", " ++ String.fromInt order.mailNumber)
         , viewInfoRow "Ordered From" order.sellerName
         , viewInfoRow "Vendor Contact" order.sellerContact
         , viewInfoRow "Cost" <| formatPrice order.price
         ]
            ++ (viewAdd order.additional
                    |> List.map (Html.map Main)
               )
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

        initialCmd res =
            getData res.token viewType
    in
    initBase [ Producer, Consumer, Manager ] (View Loading viewType showAdditional False Dict.empty Dict.empty Dict.empty) initialCmd resp


getData : String -> ViewType -> Cmd Msg
getData token viewType =
    let
        onlyMine =
            case viewType of
                ProducerView ->
                    False

                ConsumerView ->
                    True
    in
    getOrders token onlyMine



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
