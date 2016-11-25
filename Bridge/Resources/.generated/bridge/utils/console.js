﻿    Bridge.define("Bridge.Console", {
        statics: {
            BODY_WRAPPER_ID: "bridge-body-wrapper",
            CONSOLE_MESSAGES_ID: "bridge-console-messages",
            position: "horizontal",
            instance: null,
            getInstance: function () {
                if (Bridge.Console.instance == null) {
                    Bridge.Console.instance = new Bridge.Console();
                }

                return Bridge.Console.instance;
            },
            logBase: function (value, messageType) {
                if (messageType === void 0) { messageType = 0; }
                var self = Bridge.Console.getInstance();

                var v = value != null ? value.toString() : "null";

                if (self.bufferedOutput != null) {
                    self.bufferedOutput = System.String.concat(self.bufferedOutput, v);
                    return;
                }

                Bridge.Console.show();

                var m = self.buildConsoleMessage(v, messageType);
                self.consoleMessages.appendChild(m);

                self.currentMessageElement = m;

                if (self.consoleDefined) {
                    if (messageType === 1 && self.consoleDebugDefined) {
                        Bridge.global.console.debug(v);
                    } else {
                        Bridge.global.console.log(v);
                    }
                } else if (self.operaPostErrorDefined) {
                    Bridge.global.opera.postError(v);
                }
            },
            error: function (value) {
                Bridge.Console.logBase(value, 2);
            },
            debug: function (value) {
                Bridge.Console.logBase(value, 1);
            },
            log: function (value) {
                Bridge.Console.logBase(value);
            },
            hide: function () {
                if (Bridge.Console.instance == null) {
                    return;
                }

                var self = Bridge.Console.getInstance();

                if (self.hidden) {
                    return;
                }

                self.close();
            },
            show: function () {
                var self = Bridge.Console.getInstance();

                if (!self.hidden) {
                    return;
                }

                self.init(true);
            },
            toggle: function () {
                if (Bridge.Console.getInstance().hidden) {
                    Bridge.Console.show();
                } else {
                    Bridge.Console.hide();
                }
            }
        },
        svgNS: "http://www.w3.org/2000/svg",
        consoleHeight: "300px",
        consoleHeaderHeight: "35px",
        tooltip: null,
        consoleWrapper: null,
        consoleMessages: null,
        bridgeIcon: null,
        bridgeIconPath: null,
        bridgeConsoleLabel: null,
        closeBtn: null,
        closeIcon: null,
        closeIconPath: null,
        consoleHeader: null,
        consoleBody: null,
        hidden: true,
        consoleDefined: false,
        consoleDebugDefined: false,
        operaPostErrorDefined: false,
        currentMessageElement: null,
        bufferedOutput: null,
        ctor: function () {
            this.$initialize();
            this.init();
        },
        init: function (reinit) {
            if (reinit === void 0) { reinit = false; }
            this.hidden = false;

            var consoleWrapperStyles = Bridge.fn.bind(this, $_.Bridge.Console.f1)(new (System.Collections.Generic.Dictionary$2(String,String))());

            var consoleHeaderStyles = $_.Bridge.Console.f2(new (System.Collections.Generic.Dictionary$2(String,String))());

            var consoleBodyStyles = $_.Bridge.Console.f3(new (System.Collections.Generic.Dictionary$2(String,String))());

            // Bridge Icon
            this.bridgeIcon = this.bridgeIcon || document.createElementNS(this.svgNS, "svg");

            var items = Bridge.fn.bind(this, $_.Bridge.Console.f4)(new (System.Collections.Generic.Dictionary$2(String,String))());

            this.setAttributes(this.bridgeIcon, items);

            this.bridgeIconPath = this.bridgeIconPath || document.createElementNS(this.svgNS, "path");

            var items2 = new (System.Collections.Generic.Dictionary$2(String,String))();
            items2.set("d", "M19 14.4h2.2V9.6L19 7.1v7.3zm4.3-2.5v2.5h2.2l-2.2-2.5zm-8.5 2.5H17V4.8l-2.2-2.5v12.1zM0 14.4h3l7.5-8.5v8.5h2.2V0L0 14.4z");
            items2.set("fill", "#555");

            this.setAttributes(this.bridgeIconPath, items2);

            // Bridge Console Label
            this.bridgeConsoleLabel = this.bridgeConsoleLabel || document.createElement("span");
            this.bridgeConsoleLabel.innerHTML = "Bridge Console";

            // Close Button
            this.closeBtn = this.closeBtn || document.createElement("span");
            this.closeBtn.setAttribute("style", "position: relative;display: inline-block;float: right;cursor: pointer");

            this.closeIcon = this.closeIcon || document.createElementNS(this.svgNS, "svg");

            var items3 = Bridge.fn.bind(this, $_.Bridge.Console.f5)(new (System.Collections.Generic.Dictionary$2(String,String))());

            this.setAttributes(this.closeIcon, items3);

            this.closeIconPath = this.closeIconPath || document.createElementNS(this.svgNS, "path");

            var items4 = $_.Bridge.Console.f6(new (System.Collections.Generic.Dictionary$2(String,String))());

            this.setAttributes(this.closeIconPath, items4);

            this.tooltip = this.tooltip || document.createElement("div");
            this.tooltip.innerHTML = "Refresh page to open Bridge Console";

            this.tooltip.setAttribute("style", "position: absolute;right: 30px;top: -6px;white-space: nowrap;padding: 7px;border-radius: 3px;background-color: rgba(0, 0, 0, 0.75);color: #eee;text-align: center;visibility: hidden;opacity: 0;-webkit-transition: all 0.25s ease-in-out;transition: all 0.25s ease-in-out;z-index: 1;");

            // Styles and other stuff based on position
            // Force to horizontal for now
            Bridge.Console.position = "horizontal";

            if (Bridge.referenceEquals(Bridge.Console.position, "horizontal")) {
                this.wrapBodyContent();

                consoleWrapperStyles.set("right", "0");
                consoleHeaderStyles.set("border-top", "1px solid #a3a3a3");
                consoleBodyStyles.set("height", this.consoleHeight);
            } else if (Bridge.referenceEquals(Bridge.Console.position, "vertical")) {
                var consoleWidth = "400px";
                document.body.style.marginLeft = consoleWidth;

                consoleWrapperStyles.set("top", "0");
                consoleWrapperStyles.set("width", consoleWidth);
                consoleWrapperStyles.set("border-right", "1px solid #a3a3a3");
                consoleBodyStyles.set("height", "100%");
            }

            // Console wrapper
            this.consoleWrapper = this.consoleWrapper || document.createElement("div");
            this.consoleWrapper.setAttribute("style", this.obj2Css(consoleWrapperStyles));

            // Console Header
            this.consoleHeader = this.consoleHeader || document.createElement("div");
            this.consoleHeader.setAttribute("style", this.obj2Css(consoleHeaderStyles));

            // Console Body Wrapper
            this.consoleBody = this.consoleBody || document.createElement("div");
            this.consoleBody.setAttribute("style", this.obj2Css(consoleBodyStyles));

            // Console Messages Unordered List Element
            this.consoleMessages = this.consoleMessages || document.createElement("ul");
            var cm = this.consoleMessages;
            cm.id = Bridge.Console.CONSOLE_MESSAGES_ID;

            cm.setAttribute("style", "margin: 0;padding: 0;list-style: none;");

            if (!reinit) {
                this.bridgeIcon.appendChild(this.bridgeIconPath);
                this.closeIcon.appendChild(this.closeIconPath);
                this.closeBtn.appendChild(this.closeIcon);
                this.closeBtn.appendChild(this.tooltip);

                // Add child elements into console header
                this.consoleHeader.appendChild(this.bridgeIcon);
                this.consoleHeader.appendChild(this.bridgeConsoleLabel);
                this.consoleHeader.appendChild(this.closeBtn);

                // Add messages to console body
                this.consoleBody.appendChild(cm);

                // Add console header and console body into console wrapper
                this.consoleWrapper.appendChild(this.consoleHeader);
                this.consoleWrapper.appendChild(this.consoleBody);

                // Finally add console to body
                document.body.appendChild(this.consoleWrapper);

                // Close console
                this.closeBtn.addEventListener("click", Bridge.fn.bind(this, this.close));

                // Show/hide Tooltip
                this.closeBtn.addEventListener("mouseover", Bridge.fn.bind(this, this.showTooltip));
                this.closeBtn.addEventListener("mouseout", Bridge.fn.bind(this, this.hideTooltip));

                this.consoleDefined = Bridge.isDefined(Bridge.global) && Bridge.isDefined(Bridge.global.console);
                this.consoleDebugDefined = this.consoleDefined && Bridge.isDefined(Bridge.global.console.debug);
                this.operaPostErrorDefined = Bridge.isDefined(Bridge.global.opera) && Bridge.isDefined(Bridge.global.opera.postError);
            }
        },
        showTooltip: function () {
            var self = Bridge.Console.getInstance();
            self.tooltip.style.right = "20px";
            self.tooltip.style.visibility = "visible";
            self.tooltip.style.opacity = "1";
        },
        hideTooltip: function () {
            var self = Bridge.Console.getInstance();
            self.tooltip.style.right = "30px";
            self.tooltip.style.opacity = "0";
        },
        close: function () {
            this.hidden = true;

            this.consoleWrapper.style.display = "none";

            if (Bridge.referenceEquals(Bridge.Console.position, "horizontal")) {
                this.unwrapBodyContent();
            } else if (Bridge.referenceEquals(Bridge.Console.position, "vertical")) {
                document.body.removeAttribute("style");
            }
        },
        wrapBodyContent: function () {
            if (document.body == null) {
                return;
            }

            // get body margin and padding for proper alignment of scroll if a body margin/padding is used.
            var bodyStyle = document.defaultView.getComputedStyle(document.body, null);

            var bodyPaddingTop = bodyStyle.paddingTop;
            var bodyPaddingRight = bodyStyle.paddingRight;
            var bodyPaddingBottom = bodyStyle.paddingBottom;
            var bodyPaddingLeft = bodyStyle.paddingLeft;

            var bodyMarginTop = bodyStyle.marginTop;
            var bodyMarginRight = bodyStyle.marginRight;
            var bodyMarginBottom = bodyStyle.marginBottom;
            var bodyMarginLeft = bodyStyle.marginLeft;

            var div = document.createElement("div");
            div.id = Bridge.Console.BODY_WRAPPER_ID;
            div.setAttribute("style", System.String.concat("height: calc(100vh - ", this.consoleHeight, " - ", this.consoleHeaderHeight, ");", "margin-top: calc(-1 * ", "(", (System.String.concat(bodyMarginTop, " + ", bodyPaddingTop)), "));", "margin-right: calc(-1 * ", "(", (System.String.concat(bodyMarginRight, " + ", bodyPaddingRight)), "));", "margin-left: calc(-1 * ", "(", (System.String.concat(bodyMarginLeft, " + ", bodyPaddingLeft)), "));", "padding-top: calc(", (System.String.concat(bodyMarginTop, " + ", bodyPaddingTop)), ");", "padding-right: calc(", (System.String.concat(bodyMarginRight, " + ", bodyPaddingRight)), ");", "padding-bottom: calc(", (System.String.concat(bodyMarginBottom, " + ", bodyPaddingBottom)), ");", "padding-left: calc(", (System.String.concat(bodyMarginLeft, " + ", bodyPaddingLeft)), ");", "overflow-x: auto;", "box-sizing: border-box !important;"));

            while (document.body.firstChild != null) {
                div.appendChild(document.body.firstChild);
            }

            document.body.appendChild(div);
        },
        unwrapBodyContent: function () {
            var bridgeBodyWrapper = document.getElementById(Bridge.Console.BODY_WRAPPER_ID);

            if (bridgeBodyWrapper == null) {
                return;
            }

            while (bridgeBodyWrapper.firstChild != null) {
                document.body.insertBefore(bridgeBodyWrapper.firstChild, bridgeBodyWrapper);
            }

            document.body.removeChild(bridgeBodyWrapper);
        },
        buildConsoleMessage: function (message, messageType) {
            var messageItem = document.createElement("li");
            messageItem.setAttribute("style", "padding: 5px 10px;border-bottom: 1px solid #f0f0f0;");

            var messageIcon = document.createElementNS(this.svgNS, "svg");

            var items5 = Bridge.fn.bind(this, $_.Bridge.Console.f7)(new (System.Collections.Generic.Dictionary$2(String,String))());

            this.setAttributes(messageIcon, items5);

            var color = "#555";

            if (messageType === 2) {
                color = "#d65050";
            } else if (messageType === 1) {
                color = "#1800FF";
            }

            var messageIconPath = document.createElementNS(this.svgNS, "path");

            var items6 = new (System.Collections.Generic.Dictionary$2(String,String))();

            items6.set("d", "M3.8 3.5L.7 6.6s-.1.1-.2.1-.1 0-.2-.1l-.2-.3C0 6.2 0 6.2 0 6.1c0 0 0-.1.1-.1l2.6-2.6L.1.7C0 .7 0 .6 0 .6 0 .5 0 .5.1.4L.4.1c0-.1.1-.1.2-.1s.1 0 .2.1l3.1 3.1s.1.1.1.2-.1.1-.2.1z");
            items6.set("fill", color);

            this.setAttributes(messageIconPath, items6);

            messageIcon.appendChild(messageIconPath);

            var messageContainer = document.createElement("span");
            messageContainer.innerHTML = message;
            messageContainer.setAttribute("style", System.String.concat("color: ", color, "; white-space: pre;"));

            messageItem.appendChild(messageIcon);
            messageItem.appendChild(messageContainer);

            return messageItem;
        },
        setAttributes: function (el, attrs) {
            var $t;
            $t = Bridge.getEnumerator(attrs);
            while ($t.moveNext()) {
                var item = $t.getCurrent();
                el.setAttribute(item.key, item.value);
            }
        },
        obj2Css: function (obj) {
            var $t;
            var str = "";

            $t = Bridge.getEnumerator(obj);
            while ($t.moveNext()) {
                var item = $t.getCurrent();
                str = System.String.concat(str, (System.String.concat(item.key.toLowerCase(), ":", item.value, ";")));
            }

            return str;
        }
    });

    var $_ = {};

    Bridge.ns("Bridge.Console", $_);

    Bridge.apply($_.Bridge.Console, {
        f1: function (_o1) {
            _o1.add("position", "fixed");
            _o1.add("left", "0");
            _o1.add("bottom", "0");
            _o1.add("padding-top", this.consoleHeaderHeight);
            _o1.add("background-color", "#fff");
            _o1.add("font", "normal normal normal 13px/1 sans-serif");
            _o1.add("color", "#555");
            return _o1;
        },
        f2: function (_o2) {
            _o2.add("position", "absolute");
            _o2.add("top", "0");
            _o2.add("left", "0");
            _o2.add("right", "0");
            _o2.add("height", "35px");
            _o2.add("padding", "9px 15px 7px 10px");
            _o2.add("border-bottom", "1px solid #ccc");
            _o2.add("background-color", "#f3f3f3");
            _o2.add("box-sizing", "border-box");
            return _o2;
        },
        f3: function (_o3) {
            _o3.add("overflow-x", "auto");
            _o3.add("font-family", "Menlo, Monaco, Consolas, 'Courier New', monospace");
            return _o3;
        },
        f4: function (_o4) {
            _o4.add("xmlns", this.svgNS);
            _o4.add("width", "25.5");
            _o4.add("height", "14.4");
            _o4.add("viewBox", "0 0 25.5 14.4");
            _o4.add("style", "margin: 0 3px 3px 0;vertical-align:middle;");
            return _o4;
        },
        f5: function (_o5) {
            _o5.add("xmlns", this.svgNS);
            _o5.add("width", "11.4");
            _o5.add("height", "11.4");
            _o5.add("viewBox", "0 0 11.4 11.4");
            _o5.add("style", "vertical-align: middle;");
            return _o5;
        },
        f6: function (_o6) {
            _o6.add("d", "M11.4 1.4L10 0 5.7 4.3 1.4 0 0 1.4l4.3 4.3L0 10l1.4 1.4 4.3-4.3 4.3 4.3 1.4-1.4-4.3-4.3");
            _o6.add("fill", "#555");
            return _o6;
        },
        f7: function (_o7) {
            _o7.add("xmlns", this.svgNS);
            _o7.add("width", "3.9");
            _o7.add("height", "6.7");
            _o7.add("viewBox", "0 0 3.9 6.7");
            _o7.add("style", "margin-right: 7px; vertical-align: middle;");
            return _o7;
        }
    });
