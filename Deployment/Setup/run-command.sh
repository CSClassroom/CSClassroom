pushd Playbooks
export TERRAFORM_STATE_ROOT=../../Provision
export GROUP=$1
shift
export COMMAND=$@
ansible -i terraform.py linode_group=$GROUP -a "$COMMAND"
popd
