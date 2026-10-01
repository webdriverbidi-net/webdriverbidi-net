# HAR 1.2 JSON schema

The JSON schema files here are copied unchanged from the `har-schema` npm package, version 2.0.0
(https://github.com/ahmadnassri/har-schema), under its ISC license (see `LICENSE`). The HAR tests
validate generated documents against them.

The schema writes numeric bounds as `min`, which is not a JSON Schema keyword, so it does not enforce
them; the tests check those bounds themselves.
