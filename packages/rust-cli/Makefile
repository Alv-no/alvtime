.PHONY: build install uninstall clean run test

# Adjust NAME if your Cargo package produces a different binary name
NAME := atime

# Build output directory (Cargo default)
TARGET_DIR := target/release
RELEASE_BINARY := $(TARGET_DIR)/$(NAME)

# Install destination
DEST_DIR := $(HOME)/bin
DEST_BINARY := $(DEST_DIR)/$(NAME)

build:
	cargo build --release

install: build
	@mkdir -p "$(DEST_DIR)"
	@rm -f "$(DEST_BINARY)"
	@cp "$(RELEASE_BINARY)" "$(DEST_BINARY)"
	@chmod +x "$(DEST_BINARY)"
	@echo "Installed $(DEST_BINARY)"

uninstall:
	@rm -f "$(DEST_BINARY)"
	@echo "Uninstalled $(DEST_BINARY)"

clean:
	cargo clean

run: install
	"$(DEST_BINARY)"

test:
	cargo test