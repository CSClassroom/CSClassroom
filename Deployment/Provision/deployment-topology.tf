provider "linode"
{
  key = "${var.linode_api_key}"
}

resource "linode_linode" "csclassroom-webapp"
{
  image = "Ubuntu 16.04 LTS"
  kernel = "GRUB 2"
  name = "csclassroom-webapp"
  group = "webapp"
  region = "${var.linode_region}"
  size = "2048"
  private_networking = true
  ssh_key = "${var.ssh_public_key}"
  root_password="${var.initial_root_password}"
}

resource "linode_linode" "csclassroom-buildservice"
{
  count = "${var.num_buildservice_nodes}"
  image = "Ubuntu 16.04 LTS"
  kernel = "GRUB 2"
  name = "csclassroom-buildservice-${count.index + 1}"
  group = "buildservice"
  region = "${var.linode_region}"
  size = "1024"
  private_networking = true
  ssh_key = "${var.ssh_public_key}"
  root_password="${var.initial_root_password}"
}
