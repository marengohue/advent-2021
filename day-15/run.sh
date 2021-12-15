#!/bin/bash

export RUST_BACKTRACE=1 && cat ./input.txt | cargo run --release
