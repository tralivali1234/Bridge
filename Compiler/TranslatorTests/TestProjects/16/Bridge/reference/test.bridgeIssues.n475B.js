﻿Bridge.assembly("TestProject", function ($asm, globals) {
    "use strict";

    Bridge.define("Test.BridgeIssues.N475B.Bridge475Event", {
        config: {
            properties: {
                Data: 0
            }
        },
        preventDefault: function () {
            this.setData(77);
        }
    });

    Bridge.define("Test.BridgeIssues.N475B.Test", {
        statics: {
            N475: function () {
                var b = new Test.BridgeIssues.N475B.Bridge475();
                b.keyDown($_.Test.BridgeIssues.N475B.Test.f1);

                b.keyDown(4);

                b.keyDown("5");
            }
        }
    });

    var $_ = {};

    Bridge.ns("Test.BridgeIssues.N475B.Test", $_);

    Bridge.apply($_.Test.BridgeIssues.N475B.Test, {
        f1: function (ev) {
            ev.preventDefault();
        }
    });
});
