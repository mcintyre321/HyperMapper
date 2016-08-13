"use strict";

var _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; };

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

var _get = function get(_x, _x2, _x3) { var _again = true; _function: while (_again) { var object = _x, property = _x2, receiver = _x3; _again = false; if (object === null) object = Function.prototype; var desc = Object.getOwnPropertyDescriptor(object, property); if (desc === undefined) { var parent = Object.getPrototypeOf(object); if (parent === null) { return undefined; } else { _x = parent; _x2 = property; _x3 = receiver; _again = true; desc = parent = undefined; continue _function; } } else if ("value" in desc) { return desc.value; } else { var getter = desc.get; if (getter === undefined) { return undefined; } return getter.call(receiver); } } };

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var SirenField = (function (_React$Component) {
    _inherits(SirenField, _React$Component);

    function SirenField(props) {
        _classCallCheck(this, SirenField);

        _get(Object.getPrototypeOf(SirenField.prototype), "constructor", this).call(this, props);
        this.state = props;
    }

    _createClass(SirenField, [{
        key: "render",
        value: function render() {
            var id = Math.random().toString(36);
            return React.createElement(
                "div",
                null,
                React.createElement(
                    "label",
                    { htmlFor: id },
                    this.state.title || this.state.name
                ),
                React.createElement("input", { id: id, type: this.state.type, name: this.state.name, def: this.state.value || "" })
            );
        }
    }]);

    return SirenField;
})(React.Component);

var SirenAction = (function (_React$Component2) {
    _inherits(SirenAction, _React$Component2);

    function SirenAction(props) {
        _classCallCheck(this, SirenAction);

        _get(Object.getPrototypeOf(SirenAction.prototype), "constructor", this).call(this, props);
        this.state = props;
    }

    _createClass(SirenAction, [{
        key: "render",
        value: function render() {

            return React.createElement(
                "form",
                { action: this.state.href, method: this.state.method || "POST" },
                (this.state.fields || []).map(function (e) {
                    return React.createElement(SirenField, _extends({ key: e.name }, e));
                }),
                React.createElement("input", { value: this.state.name, type: "submit" })
            );
        }
    }]);

    return SirenAction;
})(React.Component);

var SirenLink = (function (_React$Component3) {
    _inherits(SirenLink, _React$Component3);

    function SirenLink(props) {
        _classCallCheck(this, SirenLink);

        _get(Object.getPrototypeOf(SirenLink.prototype), "constructor", this).call(this, props);
        this.state = props;
    }

    _createClass(SirenLink, [{
        key: "render",
        value: function render() {
            return React.createElement(
                "a",
                { href: this.state.href },
                this.state.title || this.state.href
            );
        }
    }]);

    return SirenLink;
})(React.Component);

var Siren = (function (_React$Component4) {
    _inherits(Siren, _React$Component4);

    function Siren(props) {
        _classCallCheck(this, Siren);

        _get(Object.getPrototypeOf(Siren.prototype), "constructor", this).call(this, props);
        this.state = props;
    }

    _createClass(Siren, [{
        key: "render",
        value: function render() {
            var state = this.state;
            return React.createElement(
                "div",
                { className: "entity" },
                Object.keys(state.properties || []).map(function (key) {
                    return React.createElement(
                        "div",
                        { key: key },
                        key,
                        ": ",
                        state.properties[key]
                    );
                }),
                (state.entities || []).map(function (e) {
                    if (e.href) {
                        return React.createElement(Siren, _extends({ key: e.href }, e));
                    } else {
                        var self = e.links.find(function (link) {
                            return link.rel.find(function (rel) {
                                return rel === "self";
                            });
                        });

                        return React.createElement(Siren, _extends({ key: self.href }, e));
                    }
                }),
                (state.actions || []).map(function (e) {
                    return React.createElement(SirenAction, _extends({ key: e.name || e.href }, e));
                }),
                (state.links || []).map(function (e) {
                    return React.createElement(
                        "div",
                        null,
                        React.createElement(SirenLink, _extends({ key: e.name || e.href }, e))
                    );
                })
            );
        }
    }]);

    return Siren;
})(React.Component);

