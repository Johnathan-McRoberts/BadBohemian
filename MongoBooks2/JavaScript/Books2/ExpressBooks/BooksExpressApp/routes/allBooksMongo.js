"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/*
 * allBooksMongo.ts
 * GET all books mongo.
 */
const express = require("express");
const book_1 = require("./book");
const router = express.Router();
var hostname;
router.get('/', (req, res) => {
    console.log('router.get allBooksMongo / ');
    hostname = req.hostname;
    let books = book_1.default.find((err, books) => {
        if (err) {
            console.log('error in books find - ', err);
            res.render('allBooksHack', {
                title: 'Express all books from DB error!',
                books: [
                    { author: "A bad thing", title: "but worse" },
                    { author: "is going", title: "To follow" }
                ]
            }); //.send("Error!");
        }
        else {
            res.render('allBooksHack', {
                title: 'Express got all books from DB',
                books: books
            });
            //res.send(books);
        }
    });
});
exports.default = router;
//# sourceMappingURL=allBooksMongo.js.map